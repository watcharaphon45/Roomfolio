using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Domain.Deposits;
using Roomfolio.Domain.Leasing;

namespace Roomfolio.Web.Endpoints;

public static class SecurityDepositEndpoints
{
    public static IEndpointRouteBuilder MapSecurityDepositEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/deposits").WithTags("Security Deposits");

        // GET /api/v1/deposits
        group.MapGet("/", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            [FromQuery] int? holder,
            [FromQuery] int? status,
            CancellationToken ct) =>
        {
            var query = db.SecurityDeposits
                .Include(d => d.Lease)
                    .ThenInclude(l => l!.Unit)
                        .ThenInclude(u => u!.Project)
                .Include(d => d.Lease)
                    .ThenInclude(l => l!.Tenant)
                .Include(d => d.Transactions)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(d =>
                    (d.Lease != null && d.Lease.LeaseNumber.ToLower().Contains(term)) ||
                    (d.Lease != null && d.Lease.Tenant != null && d.Lease.Tenant.FullName.ToLower().Contains(term)) ||
                    (d.Lease != null && d.Lease.Unit != null && d.Lease.Unit.UnitNumber.ToLower().Contains(term)) ||
                    (d.Lease != null && d.Lease.Unit != null && d.Lease.Unit.Project != null && d.Lease.Unit.Project.Name.ToLower().Contains(term)));
            }

            if (holder.HasValue && Enum.IsDefined(typeof(DepositHolder), holder.Value))
            {
                var h = (DepositHolder)holder.Value;
                query = query.Where(d => d.Holder == h);
            }

            if (status.HasValue && Enum.IsDefined(typeof(DepositStatus), status.Value))
            {
                var st = (DepositStatus)status.Value;
                query = query.Where(d => d.Status == st);
            }

            var deposits = await query
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync(ct);

            var items = deposits.Select(d =>
            {
                var unitDisplay = d.Lease?.Unit != null
                    ? $"{d.Lease.Unit.Project?.Name ?? "โครงการ"} {d.Lease.Unit.UnitNumber}"
                    : "ไม่ระบุห้อง";

                var leaseNo = d.Lease?.LeaseNumber ?? "N/A";
                var tenantName = d.Lease?.Tenant?.FullName ?? "ไม่ระบุผู้เช่า";

                var holderName = d.Holder switch
                {
                    DepositHolder.Agency => "Agency (นายหน้าถือ)",
                    DepositHolder.Owner => "Owner (เจ้าของถือ)",
                    _ => "อื่นๆ"
                };

                var statusName = d.Status switch
                {
                    DepositStatus.Held => "ถือครองอยู่",
                    DepositStatus.PartiallyDeducted => "หักชดเชยบางส่วน",
                    DepositStatus.FullyRefunded => "คืนเงินแล้วเสร็จ",
                    DepositStatus.Forfeited => "ริบเงินประกัน",
                    _ => "ไม่ระบุ"
                };

                var txList = d.Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Select(t => new
                    {
                        id = t.Id.ToString(),
                        type = (int)t.Type,
                        typeName = t.Type switch
                        {
                            DepositTransactionType.InitialDeposit => "รับเงินประกัน",
                            DepositTransactionType.Deduction => "หักชดเชยค่าเสียหาย/ทำความสะอาด",
                            DepositTransactionType.Refund => "คืนเงินประกันให้ผู้เช่า",
                            DepositTransactionType.Forfeit => "ริบเงินประกัน",
                            _ => "อื่นๆ"
                        },
                        amount = t.Amount,
                        date = t.TransactionDate.ToString("dd MMM yyyy"),
                        rawDate = t.TransactionDate.ToString("yyyy-MM-dd"),
                        description = t.Description,
                        referenceNo = t.ReferenceNo,
                        note = t.Note
                    })
                    .ToList();

                return new
                {
                    id = d.Id.ToString(),
                    leaseId = d.LeaseId.ToString(),
                    leaseNo,
                    unit = unitDisplay,
                    tenant = tenantName,
                    holderId = (int)d.Holder,
                    holder = holderName,
                    initialAmount = d.InitialAmount,
                    deducted = d.DeductedAmount,
                    refunded = d.RefundedAmount,
                    balance = d.Balance,
                    statusId = (int)d.Status,
                    status = statusName,
                    receivedDate = d.ReceivedDate.HasValue ? d.ReceivedDate.Value.ToString("dd MMM yyyy") : null,
                    bankAccountDetails = d.BankAccountDetails,
                    note = d.Note,
                    transactions = txList
                };
            }).ToList();

            var totalInitialAmount = items.Sum(i => i.initialAmount);
            var agencyHeldAmount = items.Where(i => i.holderId == (int)DepositHolder.Agency).Sum(i => i.balance);
            var ownerHeldAmount = items.Where(i => i.holderId == (int)DepositHolder.Owner).Sum(i => i.balance);
            var totalDeductedAmount = items.Sum(i => i.deducted);
            var totalBalance = items.Sum(i => i.balance);

            return Results.Ok(new
            {
                totalCount = items.Count,
                totalInitialAmount,
                agencyHeldAmount,
                ownerHeldAmount,
                totalDeductedAmount,
                totalBalance,
                items
            });
        })
        .WithName("GetSecurityDeposits")
        .WithOpenApi();

        // POST /api/v1/deposits/{id}/transactions (Add deduction or refund)
        group.MapPost("/{id:guid}/transactions", async (
            [FromRoute] Guid id,
            [FromServices] IRoomfolioDbContext db,
            [FromBody] AddDepositTransactionRequest req,
            CancellationToken ct) =>
        {
            if (req.Amount <= 0)
            {
                return Results.BadRequest(new { error = "จำนวนเงินต้องมากกว่า 0" });
            }

            var deposit = await db.SecurityDeposits
                .Include(d => d.Transactions)
                .FirstOrDefaultAsync(d => d.Id == id, ct);

            if (deposit == null)
            {
                return Results.NotFound(new { error = "ไม่พบรายการเงินประกัน" });
            }

            var txDate = req.TransactionDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var txType = (DepositTransactionType)req.Type;

            if (txType == DepositTransactionType.Deduction)
            {
                if (deposit.Balance < req.Amount)
                {
                    return Results.BadRequest(new { error = $"ยอดหักชดเชย (฿{req.Amount:N0}) มากกว่ายอดเงินประกันคงเหลือ (฿{deposit.Balance:N0})" });
                }
                deposit.DeductedAmount += req.Amount;
                deposit.Status = deposit.Balance <= 0 
                    ? DepositStatus.FullyRefunded 
                    : DepositStatus.PartiallyDeducted;
            }
            else if (txType == DepositTransactionType.Refund)
            {
                if (deposit.Balance < req.Amount)
                {
                    return Results.BadRequest(new { error = $"ยอดคืนเงิน (฿{req.Amount:N0}) มากกว่ายอดเงินประกันคงเหลือ (฿{deposit.Balance:N0})" });
                }
                deposit.RefundedAmount += req.Amount;
                deposit.Status = deposit.Balance <= 0 
                    ? DepositStatus.FullyRefunded 
                    : DepositStatus.PartiallyDeducted;
            }

            var tx = new DepositTransaction
            {
                SecurityDepositId = deposit.Id,
                Type = txType,
                Amount = req.Amount,
                TransactionDate = txDate,
                Description = req.Description.Trim(),
                ReferenceNo = req.ReferenceNo?.Trim(),
                Note = req.Note?.Trim()
            };

            db.DepositTransactions.Add(tx);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                success = true,
                transactionId = tx.Id,
                balance = deposit.Balance,
                status = deposit.Status.ToString()
            });
        })
        .WithName("AddDepositTransaction")
        .WithOpenApi();

        // DELETE /api/v1/deposits/{id}
        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var deposit = await db.SecurityDeposits.FindAsync(new object[] { id }, ct);
            if (deposit == null)
            {
                return Results.NotFound(new { error = "ไม่พบรายการเงินประกัน" });
            }

            db.SecurityDeposits.Remove(deposit);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { success = true });
        })
        .WithName("DeleteSecurityDeposit")
        .WithOpenApi();

        return app;
    }
}

public record AddDepositTransactionRequest(
    int Type, // 2: Deduction, 3: Refund
    decimal Amount,
    DateOnly? TransactionDate,
    string Description,
    string? ReferenceNo,
    string? Note
);
