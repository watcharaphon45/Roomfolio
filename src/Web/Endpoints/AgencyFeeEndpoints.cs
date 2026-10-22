using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Domain.Finance;

namespace Roomfolio.Web.Endpoints;

public static class AgencyFeeEndpoints
{
    public static IEndpointRouteBuilder MapAgencyFeeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/agency-fees").WithTags("Agency Fees & Commissions");

        // GET /api/v1/agency-fees
        group.MapGet("/", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            [FromQuery] int? feeType,
            [FromQuery] int? status,
            [FromQuery] int? method,
            CancellationToken ct) =>
        {
            var query = db.AgencyFees
                .Include(f => f.Unit)
                    .ThenInclude(u => u!.Project)
                .Include(f => f.Owner)
                .Include(f => f.Lease)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(f =>
                    f.Description.ToLower().Contains(term) ||
                    f.Period.ToLower().Contains(term) ||
                    (f.InvoiceNo != null && f.InvoiceNo.ToLower().Contains(term)) ||
                    (f.Owner != null && f.Owner.FullName.ToLower().Contains(term)) ||
                    (f.Unit != null && f.Unit.UnitNumber.ToLower().Contains(term)) ||
                    (f.Unit != null && f.Unit.Project != null && f.Unit.Project.Name.ToLower().Contains(term)));
            }

            if (feeType.HasValue && Enum.IsDefined(typeof(AgencyFeeType), feeType.Value))
            {
                var ft = (AgencyFeeType)feeType.Value;
                query = query.Where(f => f.FeeType == ft);
            }

            if (status.HasValue && Enum.IsDefined(typeof(AgencyFeeStatus), status.Value))
            {
                var st = (AgencyFeeStatus)status.Value;
                query = query.Where(f => f.Status == st);
            }

            if (method.HasValue && Enum.IsDefined(typeof(FeeCollectionMethod), method.Value))
            {
                var m = (FeeCollectionMethod)method.Value;
                query = query.Where(f => f.CollectionMethod == m);
            }

            var fees = await query
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync(ct);

            var items = fees.Select(f =>
            {
                var feeTypeName = f.FeeType switch
                {
                    AgencyFeeType.MonthlyManagement => "ค่าบริหารรายเดือน (Monthly Management)",
                    AgencyFeeType.LeasingCommission => "ค่านายหน้าปล่อยเช่า (Leasing Commission)",
                    AgencyFeeType.RenewalFee => "ค่าธรรมเนียมต่อสัญญา (Renewal Fee)",
                    _ => "ค่าบริการอื่นๆ"
                };

                var methodName = f.CollectionMethod switch
                {
                    FeeCollectionMethod.DeductFromSettlement => "หักจาก Settlement",
                    FeeCollectionMethod.SeparateInvoice => "ออก Invoice แยก",
                    FeeCollectionMethod.DirectPayment => "ชำระตรง",
                    _ => "อื่นๆ"
                };

                var statusName = f.Status switch
                {
                    AgencyFeeStatus.PendingDeduction => "รอนำส่ง/หักยอด",
                    AgencyFeeStatus.Invoiced => "ออกใบแจ้งหนี้แล้ว",
                    AgencyFeeStatus.Collected => "รับเงินแล้ว",
                    _ => "ยกเลิก"
                };

                var unitDisplay = f.Unit != null
                    ? $"{f.Unit.Project?.Name ?? "โครงการ"} {f.Unit.UnitNumber}"
                    : "ไม่ระบุห้อง";

                var payerDisplay = f.Owner != null
                    ? $"เจ้าของ ({f.Owner.FullName})"
                    : "เจ้าของห้อง";

                return new
                {
                    id = f.Id.ToString(),
                    feeTypeId = (int)f.FeeType,
                    feeType = feeTypeName,
                    unitId = f.UnitId?.ToString(),
                    unit = unitDisplay,
                    ownerId = f.OwnerId?.ToString(),
                    payer = payerDisplay,
                    period = f.Period,
                    calcBasis = f.CalculationBasis,
                    amount = f.Amount,
                    methodId = (int)f.CollectionMethod,
                    method = methodName,
                    statusId = (int)f.Status,
                    status = statusName,
                    invoiceNo = f.InvoiceNo,
                    collectedAt = f.CollectedAt.HasValue ? f.CollectedAt.Value.ToString("dd MMM yyyy") : null,
                    note = f.Note
                };
            }).ToList();

            var totalAmount = items.Sum(i => i.amount);
            var collectedAmount = items.Where(i => i.statusId == (int)AgencyFeeStatus.Collected).Sum(i => i.amount);
            var pendingDeductionAmount = items.Where(i => i.statusId == (int)AgencyFeeStatus.PendingDeduction).Sum(i => i.amount);
            var invoicedAmount = items.Where(i => i.statusId == (int)AgencyFeeStatus.Invoiced).Sum(i => i.amount);

            return Results.Ok(new
            {
                totalCount = items.Count,
                totalAmount,
                collectedAmount,
                pendingDeductionAmount,
                invoicedAmount,
                items
            });
        })
        .WithName("GetAgencyFees")
        .WithOpenApi();

        // POST /api/v1/agency-fees
        group.MapPost("/", async (
            [FromServices] IRoomfolioDbContext db,
            [FromBody] CreateAgencyFeeRequest req,
            CancellationToken ct) =>
        {
            if (req.Amount <= 0)
            {
                return Results.BadRequest(new { error = "จำนวนเงินต้องมากกว่า 0" });
            }

            var unit = await db.Units
                .Include(u => u.Owner)
                .FirstOrDefaultAsync(u => u.Id == req.UnitId, ct);

            if (unit == null)
            {
                return Results.BadRequest(new { error = "ไม่พบห้องชุดที่ระบุ" });
            }

            var fee = new AgencyFee
            {
                UnitId = unit.Id,
                OwnerId = unit.OwnerId,
                LeaseId = req.LeaseId,
                FeeType = (AgencyFeeType)req.FeeType,
                Description = req.Description.Trim(),
                Period = req.Period.Trim(),
                CalculationBasis = req.CalculationBasis.Trim(),
                Amount = req.Amount,
                CollectionMethod = (FeeCollectionMethod)req.CollectionMethod,
                Status = (AgencyFeeStatus)req.Status,
                InvoiceNo = req.InvoiceNo?.Trim(),
                Note = req.Note?.Trim()
            };

            if (fee.Status == AgencyFeeStatus.Collected)
            {
                fee.CollectedAt = DateTimeOffset.UtcNow;
            }

            db.AgencyFees.Add(fee);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/agency-fees/{fee.Id}", new { id = fee.Id });
        })
        .WithName("CreateAgencyFee")
        .WithOpenApi();

        // PUT /api/v1/agency-fees/{id}/status
        group.MapPut("/{id:guid}/status", async (
            [FromRoute] Guid id,
            [FromServices] IRoomfolioDbContext db,
            [FromBody] UpdateFeeStatusRequest req,
            CancellationToken ct) =>
        {
            var fee = await db.AgencyFees.FindAsync(new object[] { id }, ct);
            if (fee == null)
            {
                return Results.NotFound(new { error = "ไม่พบรายการค่าธรรมเนียม" });
            }

            fee.Status = (AgencyFeeStatus)req.Status;
            if (fee.Status == AgencyFeeStatus.Collected)
            {
                fee.CollectedAt = DateTimeOffset.UtcNow;
            }

            await db.SaveChangesAsync(ct);

            return Results.Ok(new { success = true, status = fee.Status.ToString() });
        })
        .WithName("UpdateAgencyFeeStatus")
        .WithOpenApi();

        // DELETE /api/v1/agency-fees/{id}
        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var fee = await db.AgencyFees.FindAsync(new object[] { id }, ct);
            if (fee == null)
            {
                return Results.NotFound(new { error = "ไม่พบรายการค่าธรรมเนียม" });
            }

            db.AgencyFees.Remove(fee);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { success = true });
        })
        .WithName("DeleteAgencyFee")
        .WithOpenApi();

        return app;
    }
}

public record CreateAgencyFeeRequest(
    Guid UnitId,
    Guid? LeaseId,
    int FeeType,
    string Description,
    string Period,
    string CalculationBasis,
    decimal Amount,
    int CollectionMethod,
    int Status,
    string? InvoiceNo,
    string? Note
);

public record UpdateFeeStatusRequest(
    int Status
);
