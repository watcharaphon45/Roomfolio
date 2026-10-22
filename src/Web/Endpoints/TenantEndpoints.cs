using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Application.Leasing;
using Roomfolio.Domain.Leasing;

namespace Roomfolio.Web.Endpoints;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Tenants");

        // GET /api/v1/tenants
        group.MapGet("/tenants", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            CancellationToken ct) =>
        {
            var query = db.Tenants
                .Include(t => t.Leases)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u!.Project)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(t =>
                    t.FullName.ToLower().Contains(term) ||
                    t.Phone.Contains(term) ||
                    (t.Email != null && t.Email.ToLower().Contains(term)) ||
                    (t.LineId != null && t.LineId.ToLower().Contains(term)) ||
                    (t.IdCardOrPassport != null && t.IdCardOrPassport.ToLower().Contains(term)) ||
                    t.Leases.Any(l => l.LeaseNumber.ToLower().Contains(term) ||
                                      (l.Unit != null && l.Unit.UnitNumber.ToLower().Contains(term))));
            }

            var tenants = await query
                .OrderBy(t => t.FullName)
                .ToListAsync(ct);

            var result = tenants.Select(t =>
            {
                var activeLease = t.Leases
                    .Where(l => l.Status == LeaseStatus.Active || l.Status == LeaseStatus.ExpiringSoon)
                    .OrderByDescending(l => l.EndDate)
                    .FirstOrDefault();

                string? currentUnit = null;
                if (activeLease?.Unit != null)
                {
                    var projName = activeLease.Unit.Project?.Name;
                    currentUnit = string.IsNullOrEmpty(projName)
                        ? activeLease.Unit.UnitNumber
                        : $"{projName} {activeLease.Unit.UnitNumber}";
                }

                return new TenantResponseDto(
                    t.Id,
                    t.FullName,
                    t.Phone,
                    t.Email,
                    t.IdCardOrPassport,
                    t.LineId,
                    t.EmergencyContact,
                    t.Status,
                    t.Note,
                    t.Leases.Count(l => l.Status == LeaseStatus.Active || l.Status == LeaseStatus.ExpiringSoon),
                    currentUnit,
                    activeLease?.LeaseNumber,
                    activeLease?.DepositAmount,
                    t.CreatedAt
                );
            }).ToList();

            return Results.Ok(result);
        })
        .WithName("GetTenants")
        .WithOpenApi();

        // GET /api/v1/tenants/{id}
        group.MapGet("/tenants/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var tenant = await db.Tenants
                .Include(t => t.Leases)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u!.Project)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tenant is null)
                return Results.NotFound(new { message = $"Tenant with ID {id} not found." });

            var activeLease = tenant.Leases
                .Where(l => l.Status == LeaseStatus.Active || l.Status == LeaseStatus.ExpiringSoon)
                .OrderByDescending(l => l.EndDate)
                .FirstOrDefault();

            string? currentUnit = null;
            if (activeLease?.Unit != null)
            {
                var projName = activeLease.Unit.Project?.Name;
                currentUnit = string.IsNullOrEmpty(projName)
                    ? activeLease.Unit.UnitNumber
                    : $"{projName} {activeLease.Unit.UnitNumber}";
            }

            var dto = new TenantResponseDto(
                tenant.Id,
                tenant.FullName,
                tenant.Phone,
                tenant.Email,
                tenant.IdCardOrPassport,
                tenant.LineId,
                tenant.EmergencyContact,
                tenant.Status,
                tenant.Note,
                tenant.Leases.Count(l => l.Status == LeaseStatus.Active || l.Status == LeaseStatus.ExpiringSoon),
                currentUnit,
                activeLease?.LeaseNumber,
                activeLease?.DepositAmount,
                tenant.CreatedAt
            );

            return Results.Ok(dto);
        })
        .WithName("GetTenantById")
        .WithOpenApi();

        // POST /api/v1/tenants (Create)
        group.MapPost("/tenants", async (
            [FromBody] CreateTenantRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone))
                return Results.BadRequest(new { message = "ชื่อ-นามสกุล และเบอร์โทรศัพท์ เป็นข้อมูลจำเป็น" });

            var tenant = new Tenant
            {
                FullName = request.FullName.Trim(),
                Phone = request.Phone.Trim(),
                Email = request.Email?.Trim(),
                IdCardOrPassport = request.IdCardOrPassport?.Trim(),
                LineId = request.LineId?.Trim(),
                EmergencyContact = request.EmergencyContact?.Trim(),
                Note = request.Note?.Trim(),
                Status = "Active"
            };

            db.Tenants.Add(tenant);
            await db.SaveChangesAsync(ct);

            var responseDto = new TenantResponseDto(
                tenant.Id,
                tenant.FullName,
                tenant.Phone,
                tenant.Email,
                tenant.IdCardOrPassport,
                tenant.LineId,
                tenant.EmergencyContact,
                tenant.Status,
                tenant.Note,
                0,
                null,
                null,
                null,
                tenant.CreatedAt
            );

            return Results.Created($"/api/v1/tenants/{tenant.Id}", responseDto);
        })
        .WithName("CreateTenant")
        .WithOpenApi();

        // PUT /api/v1/tenants/{id} (Update)
        group.MapPut("/tenants/{id:guid}", async (
            Guid id,
            [FromBody] UpdateTenantRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var tenant = await db.Tenants
                .Include(t => t.Leases)
                    .ThenInclude(l => l.Unit)
                        .ThenInclude(u => u!.Project)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tenant is null)
                return Results.NotFound(new { message = $"Tenant with ID {id} not found." });

            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone))
                return Results.BadRequest(new { message = "ชื่อ-นามสกุล และเบอร์โทรศัพท์ เป็นข้อมูลจำเป็น" });

            tenant.FullName = request.FullName.Trim();
            tenant.Phone = request.Phone.Trim();
            tenant.Email = request.Email?.Trim();
            tenant.IdCardOrPassport = request.IdCardOrPassport?.Trim();
            tenant.LineId = request.LineId?.Trim();
            tenant.EmergencyContact = request.EmergencyContact?.Trim();
            tenant.Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim();
            tenant.Note = request.Note?.Trim();

            await db.SaveChangesAsync(ct);

            var activeLease = tenant.Leases
                .Where(l => l.Status == LeaseStatus.Active || l.Status == LeaseStatus.ExpiringSoon)
                .OrderByDescending(l => l.EndDate)
                .FirstOrDefault();

            string? currentUnit = null;
            if (activeLease?.Unit != null)
            {
                var projName = activeLease.Unit.Project?.Name;
                currentUnit = string.IsNullOrEmpty(projName)
                    ? activeLease.Unit.UnitNumber
                    : $"{projName} {activeLease.Unit.UnitNumber}";
            }

            var responseDto = new TenantResponseDto(
                tenant.Id,
                tenant.FullName,
                tenant.Phone,
                tenant.Email,
                tenant.IdCardOrPassport,
                tenant.LineId,
                tenant.EmergencyContact,
                tenant.Status,
                tenant.Note,
                tenant.Leases.Count(l => l.Status == LeaseStatus.Active || l.Status == LeaseStatus.ExpiringSoon),
                currentUnit,
                activeLease?.LeaseNumber,
                activeLease?.DepositAmount,
                tenant.CreatedAt
            );

            return Results.Ok(responseDto);
        })
        .WithName("UpdateTenant")
        .WithOpenApi();

        // DELETE /api/v1/tenants/{id}
        group.MapDelete("/tenants/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var tenant = await db.Tenants
                .Include(t => t.Leases)
                .FirstOrDefaultAsync(t => t.Id == id, ct);

            if (tenant is null)
                return Results.NotFound(new { message = $"Tenant with ID {id} not found." });

            if (tenant.Leases.Count > 0)
            {
                return Results.BadRequest(new
                {
                    message = $"ไม่สามารถลบผู้เช่า {tenant.FullName} ได้เนื่องจากมีประวัติสัญญาเช่า {tenant.Leases.Count} ฉบับอยู่ในระบบ แนะนำให้เปลี่ยนสถานะเป็น Inactive แทน"
                });
            }

            db.Tenants.Remove(tenant);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteTenant")
        .WithOpenApi();

        return app;
    }
}
