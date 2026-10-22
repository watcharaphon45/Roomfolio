using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Web.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/dashboard").WithTags("Dashboard Metrics");

        group.MapGet("/summary", async (
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var now = DateTimeOffset.UtcNow;
            var today = DateOnly.FromDateTime(now.DateTime);
            var currentYear = today.Year;
            var currentMonth = today.Month;

            // 1. Units & Occupancy
            var totalUnits = await db.Units.CountAsync(ct);
            var availableUnits = await db.Units.CountAsync(u => u.Status == UnitStatus.Available, ct);
            var occupiedUnits = totalUnits - availableUnits;
            var occupancyRate = totalUnits > 0 ? Math.Round((double)occupiedUnits / totalUnits * 100, 1) : 0;

            // 2. Active Leases & Modes
            var activeLeases = await db.Leases
                .Include(l => l.Unit)
                    .ThenInclude(u => u!.Project)
                .Include(l => l.Tenant)
                .Where(l => l.Status == LeaseStatus.Active)
                .AsNoTracking()
                .ToListAsync(ct);

            var directLeases = activeLeases.Where(l => l.PaymentMode == RentalPaymentMode.DirectToOwner).ToList();
            var throughAgentLeases = activeLeases.Where(l => l.PaymentMode == RentalPaymentMode.ThroughAgent).ToList();

            var directModeCount = directLeases.Count;
            var directModeAmount = directLeases.Sum(l => l.MonthlyRent);
            var throughAgentCount = throughAgentLeases.Count;
            var throughAgentAmount = throughAgentLeases.Sum(l => l.MonthlyRent);

            var totalModeCount = directModeCount + throughAgentCount;
            var directModePct = totalModeCount > 0 ? Math.Round((double)directModeCount / totalModeCount * 100) : 0;
            var throughAgentPct = totalModeCount > 0 ? Math.Round((double)throughAgentCount / totalModeCount * 100) : 0;

            // 3. Agency Cash Held (payments received through agent minus settled amounts)
            var throughAgentPayments = await db.Payments
                .Where(p => p.ReceivedByParty == PaymentParty.Agency)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var paidSettlementGross = await db.OwnerSettlements
                .Where(s => s.Status == SettlementStatus.Paid)
                .SumAsync(s => (decimal?)s.GrossRentCollected, ct) ?? 0m;

            var agencyCashHeld = Math.Max(0, throughAgentPayments - paidSettlementGross);
            var cashHeldCount = throughAgentCount;

            // 4. Owners pending settlement
            var pendingSettlements = await db.OwnerSettlements
                .Include(s => s.Owner)
                .Where(s => s.Status == SettlementStatus.Draft || s.Status == SettlementStatus.Review || s.Status == SettlementStatus.Approved)
                .AsNoTracking()
                .ToListAsync(ct);

            var ownersPending = pendingSettlements.Select(s => s.OwnerId).Distinct().Count();
            if (ownersPending == 0 && throughAgentCount > 0)
            {
                ownersPending = throughAgentLeases.Select(l => l.Unit?.OwnerId).Where(id => id.HasValue).Distinct().Count();
            }

            // 5. Rent Charged This Month & Collected
            var monthCharges = await db.RentCharges
                .Where(c => c.DueDate.Year == currentYear && c.DueDate.Month == currentMonth)
                .AsNoTracking()
                .ToListAsync(ct);

            decimal rentChargedThisMonth;
            if (monthCharges.Any())
            {
                rentChargedThisMonth = monthCharges.Sum(c => c.Amount);
            }
            else
            {
                rentChargedThisMonth = activeLeases.Sum(l => l.MonthlyRent);
            }

            var monthPayments = await db.Payments
                .Where(p => p.PaymentDate.Year == currentYear && p.PaymentDate.Month == currentMonth)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var rentCollectedPct = rentChargedThisMonth > 0
                ? Math.Min(100, Math.Round((double)(monthPayments / rentChargedThisMonth) * 100, 1))
                : 0;

            var rentOutstanding = Math.Max(0, rentChargedThisMonth - monthPayments);

            // 6. Agency Fees
            var agencyFeeReceivable = Math.Round(activeLeases.Sum(l => l.MonthlyRent * 0.05m), 0);
            var agencyFeeCount = activeLeases.Count;
            var agencyFeeDeductible = pendingSettlements.Sum(s => s.ManagementFeesDeducted + s.CommissionsDeducted);
            if (agencyFeeDeductible == 0 && throughAgentCount > 0)
            {
                agencyFeeDeductible = Math.Round(throughAgentAmount * 0.05m, 0);
            }

            // 7. Expiring Leases
            var expiringLeases = activeLeases
                .OrderBy(l => l.EndDate)
                .Take(4)
                .Select(l => new
                {
                    unitNumber = l.Unit != null ? $"{l.Unit.Building} {l.Unit.UnitNumber}" : "ห้องชุด",
                    project = l.Unit?.Project?.Name ?? "โครงการ",
                    tenant = l.Tenant?.FullName ?? "ผู้เช่า",
                    rent = l.MonthlyRent,
                    daysLeft = Math.Max(0, l.EndDate.DayNumber - today.DayNumber),
                    endDate = l.EndDate.ToString("dd MMM yyyy")
                })
                .ToList();

            // 8. Settlement Previews
            var settlementsList = await db.OwnerSettlements
                .Include(s => s.Owner)
                    .ThenInclude(o => o!.Units)
                .Include(s => s.Lines)
                .OrderByDescending(s => s.PeriodEnd)
                .Take(5)
                .AsNoTracking()
                .ToListAsync(ct);

            var settlementPreviews = settlementsList.Select(s =>
            {
                var (statusLabel, statusClass) = s.Status switch
                {
                    SettlementStatus.Draft => ("แบบร่าง", "badge-gray"),
                    SettlementStatus.Review => ("รอตรวจสอบ", "badge-amber"),
                    SettlementStatus.Approved => ("อนุมัติแล้ว", "badge-blue"),
                    SettlementStatus.Paid => ("โอนแล้ว", "badge-emerald"),
                    _ => ("ยกเลิก", "badge-rose")
                };

                var bankInfo = s.Owner != null && !string.IsNullOrEmpty(s.Owner.BankName)
                    ? $"{s.Owner.BankName} ··· {(s.Owner.BankAccountNumber?.Length > 4 ? s.Owner.BankAccountNumber[^4..] : s.Owner.BankAccountNumber)}"
                    : "ไม่มีข้อมูลบัญชี";

                return new
                {
                    ownerName = s.Owner?.FullName ?? "ไม่ระบุเจ้าของ",
                    bankAccount = bankInfo,
                    unitsCount = s.Owner?.Units?.Count ?? s.Lines.Select(l => l.UnitId).Distinct().Count(),
                    grossRent = s.GrossRentCollected,
                    expenses = s.ExpensesDeducted,
                    managementFee = s.ManagementFeesDeducted,
                    netPayable = s.NetPayable,
                    statusLabel,
                    statusClass
                };
            }).ToList();

            return Results.Ok(new
            {
                totalUnits,
                occupancyRate,
                availableUnits,
                agencyCashHeld,
                cashHeldCount,
                ownersPending,
                rentChargedThisMonth,
                rentCollectedPct,
                rentOutstanding,
                agencyFeeReceivable,
                agencyFeeCount,
                agencyFeeDeductible,
                directModeCount,
                directModePct,
                directModeAmount,
                throughAgentCount,
                throughAgentPct,
                throughAgentAmount,
                expiringLeases,
                settlementPreviews
            });
        })
        .WithName("GetDashboardSummary")
        .WithOpenApi();

        return app;
    }
}
