using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Application.Leasing;
using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Web.Endpoints;

public static class LeaseEndpoints
{
    public static IEndpointRouteBuilder MapLeaseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Leases & Agreements");

        // GET /api/v1/leases
        group.MapGet("/leases", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            [FromQuery] int? paymentMode,
            [FromQuery] int? status,
            CancellationToken ct) =>
        {
            var query = db.Leases
                .Include(l => l.Unit)
                    .ThenInclude(u => u!.Project)
                .Include(l => l.Tenant)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(l =>
                    l.LeaseNumber.ToLower().Contains(term) ||
                    (l.Tenant != null && l.Tenant.FullName.ToLower().Contains(term)) ||
                    (l.Unit != null && l.Unit.UnitNumber.ToLower().Contains(term)) ||
                    (l.Unit != null && l.Unit.Project != null && l.Unit.Project.Name.ToLower().Contains(term)));
            }

            if (paymentMode.HasValue && Enum.IsDefined(typeof(RentalPaymentMode), paymentMode.Value))
            {
                var mode = (RentalPaymentMode)paymentMode.Value;
                query = query.Where(l => l.PaymentMode == mode);
            }

            if (status.HasValue && Enum.IsDefined(typeof(LeaseStatus), status.Value))
            {
                var st = (LeaseStatus)status.Value;
                query = query.Where(l => l.Status == st);
            }

            var leases = await query
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => new LeaseResponseDto(
                    l.Id,
                    l.UnitId,
                    l.Unit != null ? l.Unit.UnitNumber : "-",
                    l.Unit != null && l.Unit.Project != null ? l.Unit.Project.Name : "-",
                    l.TenantId,
                    l.Tenant != null ? l.Tenant.FullName : "-",
                    l.Tenant != null ? l.Tenant.Phone : "-",
                    l.LeaseNumber,
                    l.StartDate,
                    l.EndDate,
                    l.RentDueDay,
                    l.MonthlyRent,
                    l.PaymentMode,
                    l.PaymentMode == RentalPaymentMode.ThroughAgent ? "ผ่านนายหน้า" : "จ่ายตรงเจ้าของ",
                    l.DepositAmount,
                    l.DepositHolder,
                    l.DepositHolder == DepositHolder.Agency ? "นายหน้าถือ" : (l.DepositHolder == DepositHolder.Owner ? "เจ้าของถือ" : "อื่นๆ"),
                    l.Status,
                    l.Status == LeaseStatus.Active ? "ใช้งานอยู่" :
                    (l.Status == LeaseStatus.ExpiringSoon ? "ใกล้หมดอายุ" :
                    (l.Status == LeaseStatus.Draft ? "ฉบับร่าง" :
                    (l.Status == LeaseStatus.Ended ? "สิ้นสุดสัญญา" : "ยกเลิกก่อนกำหนด"))),
                    l.SignedAt,
                    l.Note,
                    l.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(leases);
        })
        .WithName("GetLeases")
        .WithOpenApi();

        // GET /api/v1/leases/{id}
        group.MapGet("/leases/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var lease = await db.Leases
                .Include(l => l.Unit)
                    .ThenInclude(u => u!.Project)
                .Include(l => l.Tenant)
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id, ct);

            if (lease is null)
                return Results.NotFound(new { message = $"Lease with ID {id} not found." });

            var dto = new LeaseResponseDto(
                lease.Id,
                lease.UnitId,
                lease.Unit != null ? lease.Unit.UnitNumber : "-",
                lease.Unit != null && lease.Unit.Project != null ? lease.Unit.Project.Name : "-",
                lease.TenantId,
                lease.Tenant != null ? lease.Tenant.FullName : "-",
                lease.Tenant != null ? lease.Tenant.Phone : "-",
                lease.LeaseNumber,
                lease.StartDate,
                lease.EndDate,
                lease.RentDueDay,
                lease.MonthlyRent,
                lease.PaymentMode,
                lease.PaymentMode == RentalPaymentMode.ThroughAgent ? "ผ่านนายหน้า" : "จ่ายตรงเจ้าของ",
                lease.DepositAmount,
                lease.DepositHolder,
                lease.DepositHolder == DepositHolder.Agency ? "นายหน้าถือ" : (lease.DepositHolder == DepositHolder.Owner ? "เจ้าของถือ" : "อื่นๆ"),
                lease.Status,
                lease.Status == LeaseStatus.Active ? "ใช้งานอยู่" :
                (lease.Status == LeaseStatus.ExpiringSoon ? "ใกล้หมดอายุ" :
                (lease.Status == LeaseStatus.Draft ? "ฉบับร่าง" :
                (lease.Status == LeaseStatus.Ended ? "สิ้นสุดสัญญา" : "ยกเลิกก่อนกำหนด"))),
                lease.SignedAt,
                lease.Note,
                lease.CreatedAt
            );

            return Results.Ok(dto);
        })
        .WithName("GetLeaseById")
        .WithOpenApi();

        // POST /api/v1/leases (Create)
        group.MapPost("/leases", async (
            [FromBody] CreateLeaseRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.LeaseNumber))
                return Results.BadRequest(new { message = "เลขที่สัญญาเป็นข้อมูลจำเป็น" });

            if (request.MonthlyRent <= 0)
                return Results.BadRequest(new { message = "ค่าเช่าต่อเดือนต้องมากกว่า 0" });

            if (request.EndDate <= request.StartDate)
                return Results.BadRequest(new { message = "วันสิ้นสุดสัญญาต้องหลังวันเริ่มต้นสัญญา" });

            var unit = await db.Units
                .Include(u => u.Project)
                .FirstOrDefaultAsync(u => u.Id == request.UnitId, ct);

            if (unit is null)
                return Results.BadRequest(new { message = "ไม่พบห้องชุดที่ระบุ" });

            var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, ct);
            if (tenant is null)
                return Results.BadRequest(new { message = "ไม่พบข้อมูลผู้เช่าที่ระบุ" });

            var lease = new Lease
            {
                UnitId = request.UnitId,
                TenantId = request.TenantId,
                LeaseNumber = request.LeaseNumber.Trim(),
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                RentDueDay = request.RentDueDay is >= 1 and <= 31 ? request.RentDueDay : 1,
                MonthlyRent = request.MonthlyRent,
                PaymentMode = request.PaymentMode,
                DepositAmount = request.DepositAmount,
                DepositHolder = request.DepositHolder,
                Status = request.Status,
                SignedAt = request.SignedAt,
                Note = request.Note?.Trim()
            };

            // Update Unit state
            if (lease.Status == LeaseStatus.Active || lease.Status == LeaseStatus.ExpiringSoon)
            {
                unit.Status = UnitStatus.Rented;
                unit.TenantName = tenant.FullName;
                unit.PaymentMode = lease.PaymentMode;
            }

            db.Leases.Add(lease);
            await db.SaveChangesAsync(ct);

            var responseDto = new LeaseResponseDto(
                lease.Id,
                lease.UnitId,
                unit.UnitNumber,
                unit.Project?.Name ?? "-",
                lease.TenantId,
                tenant.FullName,
                tenant.Phone,
                lease.LeaseNumber,
                lease.StartDate,
                lease.EndDate,
                lease.RentDueDay,
                lease.MonthlyRent,
                lease.PaymentMode,
                lease.PaymentMode == RentalPaymentMode.ThroughAgent ? "ผ่านนายหน้า" : "จ่ายตรงเจ้าของ",
                lease.DepositAmount,
                lease.DepositHolder,
                lease.DepositHolder == DepositHolder.Agency ? "นายหน้าถือ" : (lease.DepositHolder == DepositHolder.Owner ? "เจ้าของถือ" : "อื่นๆ"),
                lease.Status,
                lease.Status == LeaseStatus.Active ? "ใช้งานอยู่" :
                (lease.Status == LeaseStatus.ExpiringSoon ? "ใกล้หมดอายุ" :
                (lease.Status == LeaseStatus.Draft ? "ฉบับร่าง" :
                (lease.Status == LeaseStatus.Ended ? "สิ้นสุดสัญญา" : "ยกเลิกก่อนกำหนด"))),
                lease.SignedAt,
                lease.Note,
                lease.CreatedAt
            );

            return Results.Created($"/api/v1/leases/{lease.Id}", responseDto);
        })
        .WithName("CreateLease")
        .WithOpenApi();

        // PUT /api/v1/leases/{id} (Update)
        group.MapPut("/leases/{id:guid}", async (
            Guid id,
            [FromBody] UpdateLeaseRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var lease = await db.Leases
                .Include(l => l.Unit)
                    .ThenInclude(u => u!.Project)
                .Include(l => l.Tenant)
                .FirstOrDefaultAsync(l => l.Id == id, ct);

            if (lease is null)
                return Results.NotFound(new { message = $"Lease with ID {id} not found." });

            if (string.IsNullOrWhiteSpace(request.LeaseNumber))
                return Results.BadRequest(new { message = "เลขที่สัญญาเป็นข้อมูลจำเป็น" });

            if (request.MonthlyRent <= 0)
                return Results.BadRequest(new { message = "ค่าเช่าต่อเดือนต้องมากกว่า 0" });

            if (request.EndDate <= request.StartDate)
                return Results.BadRequest(new { message = "วันสิ้นสุดสัญญาต้องหลังวันเริ่มต้นสัญญา" });

            var unit = await db.Units
                .Include(u => u.Project)
                .FirstOrDefaultAsync(u => u.Id == request.UnitId, ct);

            if (unit is null)
                return Results.BadRequest(new { message = "ไม่พบห้องชุดที่ระบุ" });

            var tenant = await db.Tenants.FirstOrDefaultAsync(t => t.Id == request.TenantId, ct);
            if (tenant is null)
                return Results.BadRequest(new { message = "ไม่พบข้อมูลผู้เช่าที่ระบุ" });

            lease.UnitId = request.UnitId;
            lease.TenantId = request.TenantId;
            lease.LeaseNumber = request.LeaseNumber.Trim();
            lease.StartDate = request.StartDate;
            lease.EndDate = request.EndDate;
            lease.RentDueDay = request.RentDueDay is >= 1 and <= 31 ? request.RentDueDay : 1;
            lease.MonthlyRent = request.MonthlyRent;
            lease.PaymentMode = request.PaymentMode;
            lease.DepositAmount = request.DepositAmount;
            lease.DepositHolder = request.DepositHolder;
            lease.Status = request.Status;
            lease.SignedAt = request.SignedAt;
            lease.TerminatedAt = request.TerminatedAt;
            lease.TerminationReason = request.TerminationReason?.Trim();
            lease.Note = request.Note?.Trim();

            // Sync with unit if active
            if (lease.Status == LeaseStatus.Active || lease.Status == LeaseStatus.ExpiringSoon)
            {
                unit.Status = UnitStatus.Rented;
                unit.TenantName = tenant.FullName;
                unit.PaymentMode = lease.PaymentMode;
            }

            await db.SaveChangesAsync(ct);

            var responseDto = new LeaseResponseDto(
                lease.Id,
                lease.UnitId,
                unit.UnitNumber,
                unit.Project?.Name ?? "-",
                lease.TenantId,
                tenant.FullName,
                tenant.Phone,
                lease.LeaseNumber,
                lease.StartDate,
                lease.EndDate,
                lease.RentDueDay,
                lease.MonthlyRent,
                lease.PaymentMode,
                lease.PaymentMode == RentalPaymentMode.ThroughAgent ? "ผ่านนายหน้า" : "จ่ายตรงเจ้าของ",
                lease.DepositAmount,
                lease.DepositHolder,
                lease.DepositHolder == DepositHolder.Agency ? "นายหน้าถือ" : (lease.DepositHolder == DepositHolder.Owner ? "เจ้าของถือ" : "อื่นๆ"),
                lease.Status,
                lease.Status == LeaseStatus.Active ? "ใช้งานอยู่" :
                (lease.Status == LeaseStatus.ExpiringSoon ? "ใกล้หมดอายุ" :
                (lease.Status == LeaseStatus.Draft ? "ฉบับร่าง" :
                (lease.Status == LeaseStatus.Ended ? "สิ้นสุดสัญญา" : "ยกเลิกก่อนกำหนด"))),
                lease.SignedAt,
                lease.Note,
                lease.CreatedAt
            );

            return Results.Ok(responseDto);
        })
        .WithName("UpdateLease")
        .WithOpenApi();

        // DELETE /api/v1/leases/{id}
        group.MapDelete("/leases/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var lease = await db.Leases
                .Include(l => l.Unit)
                .FirstOrDefaultAsync(l => l.Id == id, ct);

            if (lease is null)
                return Results.NotFound(new { message = $"Lease with ID {id} not found." });

            var unitId = lease.UnitId;
            db.Leases.Remove(lease);
            await db.SaveChangesAsync(ct);

            // Check if there are remaining active leases on the unit
            var hasOtherActiveLeases = await db.Leases.AnyAsync(l =>
                l.UnitId == unitId &&
                (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.ExpiringSoon), ct);

            if (!hasOtherActiveLeases && lease.Unit != null)
            {
                lease.Unit.Status = UnitStatus.Available;
                lease.Unit.TenantName = null;
                await db.SaveChangesAsync(ct);
            }

            return Results.NoContent();
        })
        .WithName("DeleteLease")
        .WithOpenApi();

        return app;
    }
}
