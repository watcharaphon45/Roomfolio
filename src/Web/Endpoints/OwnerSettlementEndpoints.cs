using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Application.Finance;
using Roomfolio.Domain.Finance;

namespace Roomfolio.Web.Endpoints;

public static class OwnerSettlementEndpoints
{
    public static IEndpointRouteBuilder MapOwnerSettlementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Owner Settlements & Statements");

        // GET /api/v1/owner-settlements
        group.MapGet("/owner-settlements", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            [FromQuery] int? status,
            CancellationToken ct) =>
        {
            var query = db.OwnerSettlements
                .Include(s => s.Owner)
                    .ThenInclude(o => o!.Units)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Unit)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(s =>
                    (s.Owner != null && s.Owner.FullName.ToLower().Contains(term)) ||
                    (s.Owner != null && s.Owner.Phone.Contains(term)) ||
                    (s.TransferReference != null && s.TransferReference.ToLower().Contains(term)));
            }

            if (status.HasValue && Enum.IsDefined(typeof(SettlementStatus), status.Value))
            {
                var st = (SettlementStatus)status.Value;
                query = query.Where(s => s.Status == st);
            }

            var allSettlements = await query
                .OrderByDescending(s => s.PeriodEnd)
                .ToListAsync(ct);

            var items = allSettlements.Select(s =>
            {
                var periodStr = $"{s.PeriodStart:dd/MM} - {s.PeriodEnd:dd/MM/yyyy}";
                var (statusText, statusBadge) = s.Status switch
                {
                    SettlementStatus.Draft => ("แบบร่าง", "badge-gray"),
                    SettlementStatus.Review => ("รอตรวจทาน", "badge-amber"),
                    SettlementStatus.Approved => ("อนุมัติแล้ว", "badge-blue"),
                    SettlementStatus.Paid => ("โอนแล้วเสร็จ", "badge-emerald"),
                    _ => ("ยกเลิก", "badge-rose")
                };

                var linesDto = s.Lines.Select(l => new SettlementLineDto(
                    l.Id,
                    l.LineType,
                    l.LineType switch
                    {
                        SettlementLineType.RentCollected => "ค่าเช่ารับจริง",
                        SettlementLineType.ManagementFee => "ค่าบริหารจัดการ",
                        SettlementLineType.Expense => "ค่าใช้จ่าย",
                        SettlementLineType.Commission => "ค่านายหน้า",
                        _ => "ปรับปรุงยอด"
                    },
                    l.UnitId,
                    l.Unit?.UnitNumber,
                    l.Description,
                    l.Amount,
                    l.Note
                )).ToList();

                return new OwnerSettlementResponseDto(
                    s.Id,
                    s.OwnerId,
                    s.Owner?.FullName ?? "-",
                    s.Owner?.Phone ?? "-",
                    s.Owner?.BankName,
                    s.Owner?.BankAccountNumber,
                    periodStr,
                    s.PeriodStart,
                    s.PeriodEnd,
                    s.Owner?.Units.Count ?? 0,
                    s.GrossRentCollected,
                    s.ExpensesDeducted,
                    s.ManagementFeesDeducted,
                    s.NetPayable,
                    s.Status,
                    statusText,
                    statusBadge,
                    s.TransferReference,
                    s.PaidAt,
                    s.Note,
                    linesDto,
                    s.CreatedAt
                );
            }).ToList();

            var totalNet = items.Sum(i => i.Net);
            var draftCount = allSettlements.Count(s => s.Status == SettlementStatus.Draft);
            var reviewCount = allSettlements.Count(s => s.Status == SettlementStatus.Review);
            var approvedCount = allSettlements.Count(s => s.Status == SettlementStatus.Approved);
            var paidCount = allSettlements.Count(s => s.Status == SettlementStatus.Paid);

            var summary = new SettlementPipelineSummaryDto(
                draftCount,
                reviewCount,
                approvedCount,
                paidCount,
                totalNet,
                items
            );

            return Results.Ok(summary);
        })
        .WithName("GetOwnerSettlements")
        .WithOpenApi();

        // GET /api/v1/owner-settlements/{id}
        group.MapGet("/owner-settlements/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var s = await db.OwnerSettlements
                .Include(s => s.Owner)
                    .ThenInclude(o => o!.Units)
                .Include(s => s.Lines)
                    .ThenInclude(l => l.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (s is null)
                return Results.NotFound(new { message = $"Settlement with ID {id} not found." });

            var periodStr = $"{s.PeriodStart:dd/MM} - {s.PeriodEnd:dd/MM/yyyy}";
            var (statusText, statusBadge) = s.Status switch
            {
                SettlementStatus.Draft => ("แบบร่าง", "badge-gray"),
                SettlementStatus.Review => ("รอตรวจทาน", "badge-amber"),
                SettlementStatus.Approved => ("อนุมัติแล้ว", "badge-blue"),
                SettlementStatus.Paid => ("โอนแล้วเสร็จ", "badge-emerald"),
                _ => ("ยกเลิก", "badge-rose")
            };

            var linesDto = s.Lines.Select(l => new SettlementLineDto(
                l.Id,
                l.LineType,
                l.LineType.ToString(),
                l.UnitId,
                l.Unit?.UnitNumber,
                l.Description,
                l.Amount,
                l.Note
            )).ToList();

            var dto = new OwnerSettlementResponseDto(
                s.Id,
                s.OwnerId,
                s.Owner?.FullName ?? "-",
                s.Owner?.Phone ?? "-",
                s.Owner?.BankName,
                s.Owner?.BankAccountNumber,
                periodStr,
                s.PeriodStart,
                s.PeriodEnd,
                s.Owner?.Units.Count ?? 0,
                s.GrossRentCollected,
                s.ExpensesDeducted,
                s.ManagementFeesDeducted,
                s.NetPayable,
                s.Status,
                statusText,
                statusBadge,
                s.TransferReference,
                s.PaidAt,
                s.Note,
                linesDto,
                s.CreatedAt
            );

            return Results.Ok(dto);
        })
        .WithName("GetOwnerSettlementById")
        .WithOpenApi();

        // POST /api/v1/owner-settlements (Create / Generate Settlement)
        group.MapPost("/owner-settlements", async (
            [FromBody] CreateSettlementRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var owner = await db.Owners
                .Include(o => o.Units)
                .FirstOrDefaultAsync(o => o.Id == request.OwnerId, ct);

            if (owner is null)
                return Results.BadRequest(new { message = "ไม่พบข้อมูลเจ้าของห้องที่ระบุ" });

            if (request.GrossRentCollected < 0)
                return Results.BadRequest(new { message = "ยอดค่าเช่ารับจริงต้องไม่ติดลบ" });

            var feeAmount = Math.Round(request.GrossRentCollected * (request.ManagementFeePercent / 100m), 2);
            var netPayable = request.GrossRentCollected - request.ExpensesDeducted - feeAmount;

            var settlement = new OwnerSettlement
            {
                OwnerId = owner.Id,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                Status = SettlementStatus.Review,
                GrossRentCollected = request.GrossRentCollected,
                ExpensesDeducted = request.ExpensesDeducted,
                ManagementFeesDeducted = feeAmount,
                CommissionsDeducted = 0,
                OtherAdjustments = 0,
                NetPayable = netPayable,
                Note = request.Note?.Trim()
            };

            db.OwnerSettlements.Add(settlement);
            await db.SaveChangesAsync(ct);

            // Create initial statement lines
            if (request.GrossRentCollected > 0)
            {
                db.OwnerSettlementLines.Add(new OwnerSettlementLine
                {
                    OwnerSettlementId = settlement.Id,
                    LineType = SettlementLineType.RentCollected,
                    Description = $"ค่าเช่ารับจริงประจำงวด {request.PeriodStart:dd/MM} - {request.PeriodEnd:dd/MM/yyyy}",
                    Amount = request.GrossRentCollected
                });
            }

            if (feeAmount > 0)
            {
                db.OwnerSettlementLines.Add(new OwnerSettlementLine
                {
                    OwnerSettlementId = settlement.Id,
                    LineType = SettlementLineType.ManagementFee,
                    Description = $"ค่าธรรมเนียมบริหารจัดการปล่อยเช่า ({request.ManagementFeePercent}%)",
                    Amount = -feeAmount
                });
            }

            if (request.ExpensesDeducted > 0)
            {
                db.OwnerSettlementLines.Add(new OwnerSettlementLine
                {
                    OwnerSettlementId = settlement.Id,
                    LineType = SettlementLineType.Expense,
                    Description = "หักค่าใช้จ่ายซ่อมแซม/ทำความสะอาดห้องชุด",
                    Amount = -request.ExpensesDeducted
                });
            }

            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/owner-settlements/{settlement.Id}", new { id = settlement.Id });
        })
        .WithName("CreateOwnerSettlement")
        .WithOpenApi();

        // PUT /api/v1/owner-settlements/{id}/status (Update Status & Transfer Ref)
        group.MapPut("/owner-settlements/{id:guid}/status", async (
            Guid id,
            [FromBody] UpdateSettlementStatusRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var settlement = await db.OwnerSettlements
                .Include(s => s.Owner)
                .FirstOrDefaultAsync(s => s.Id == id, ct);

            if (settlement is null)
                return Results.NotFound(new { message = $"Settlement with ID {id} not found." });

            settlement.Status = request.Status;

            if (request.Status == SettlementStatus.Approved)
            {
                settlement.ApprovedAt = DateTimeOffset.UtcNow;
            }
            else if (request.Status == SettlementStatus.Paid)
            {
                settlement.PaidAt = DateTimeOffset.UtcNow;
                if (!string.IsNullOrWhiteSpace(request.TransferReference))
                {
                    settlement.TransferReference = request.TransferReference.Trim();
                }
            }

            if (!string.IsNullOrWhiteSpace(request.Note))
            {
                settlement.Note = request.Note.Trim();
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { message = $"อัปเดตสถานะรอบโอนเงินเป็น {settlement.Status} สำเร็จ" });
        })
        .WithName("UpdateOwnerSettlementStatus")
        .WithOpenApi();

        // DELETE /api/v1/owner-settlements/{id}
        group.MapDelete("/owner-settlements/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var settlement = await db.OwnerSettlements.FirstOrDefaultAsync(s => s.Id == id, ct);
            if (settlement is null)
                return Results.NotFound(new { message = $"Settlement with ID {id} not found." });

            if (settlement.Status == SettlementStatus.Paid)
            {
                return Results.BadRequest(new { message = "ไม่สามารถลบรอบที่โอนเงินแล้วเสร็จได้" });
            }

            db.OwnerSettlements.Remove(settlement);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteOwnerSettlement")
        .WithOpenApi();

        return app;
    }
}
