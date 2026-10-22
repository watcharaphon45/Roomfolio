using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Web.Endpoints;

public static class ReportEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reports").WithTags("Reports & Exports");

        // GET /api/v1/reports/summary
        group.MapGet("/summary", async ([FromServices] IRoomfolioDbContext db, CancellationToken ct) =>
        {
            var now = DateTimeOffset.UtcNow;
            var today = DateOnly.FromDateTime(now.Date);
            var in90Days = today.AddDays(90);

            var totalUnits = await db.Units.CountAsync(ct);
            var settlementCount = await db.OwnerSettlements.CountAsync(ct);
            var totalPaidToOwners = await db.OwnerSettlements
                .Where(s => s.Status == SettlementStatus.Paid)
                .SumAsync(s => (decimal?)s.NetPayable, ct) ?? 0m;

            var agencyCashHeld = await db.Payments
                .Where(p => p.ReceivedByParty == PaymentParty.Agency)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var agencyPaidToOwners = await db.OwnerSettlements
                .Where(s => s.Status == SettlementStatus.Paid)
                .SumAsync(s => (decimal?)s.NetPayable, ct) ?? 0m;

            var netAgencyCash = Math.Max(0, agencyCashHeld - agencyPaidToOwners);

            var overdueCharges = await db.RentCharges
                .Include(c => c.Allocations)
                .Where(c => c.DueDate < today && c.Status != RentChargeStatus.Paid)
                .ToListAsync(ct);

            var overdueCount = overdueCharges.Count;
            var overdueAmount = overdueCharges.Sum(c => c.Amount - c.Allocations.Sum(a => a.AllocatedAmount));

            var expiringLeasesCount = await db.Leases
                .Where(l => l.Status == LeaseStatus.Active && l.EndDate <= in90Days && l.EndDate >= today)
                .CountAsync(ct);

            return Results.Ok(new
            {
                totalUnits,
                settlementCount,
                totalPaidToOwners,
                agencyCashHeld = netAgencyCash,
                overdueCount,
                overdueAmount,
                expiringLeasesCount
            });
        });

        // GET /api/v1/reports/owner-statements/export-csv
        group.MapGet("/owner-statements/export-csv", async ([FromServices] IRoomfolioDbContext db, CancellationToken ct) =>
        {
            var settlements = await db.OwnerSettlements
                .Include(s => s.Owner)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(ct);

            var sb = new StringBuilder();
            sb.Append('\uFEFF'); // UTF-8 BOM for Microsoft Excel
            sb.AppendLine("รหัสรอบการโอน,เจ้าของห้อง,เบอร์โทรศัพท์,ธนาคาร,เลขบัญชี,งวดเริ่ม,งวดสิ้นสุด,ยอดค่าเช่ารวม (บาท),รายการหักค่าใช้จ่าย (บาท),ค่าบริการเอเจนซี่ (บาท),ยอดสุทธิโอน (บาท),สถานะ,วันที่ชำระ,เลขอ้างอิงโอน");

            foreach (var s in settlements)
            {
                var ownerName = EscapeCsv(s.Owner?.FullName ?? "-");
                var phone = EscapeCsv(s.Owner?.Phone ?? "-");
                var bank = EscapeCsv(s.Owner?.BankName ?? "-");
                var acc = EscapeCsv(s.Owner?.BankAccountNumber ?? "-");
                var status = s.Status switch
                {
                    SettlementStatus.Draft => "ร่างเอกสาร",
                    SettlementStatus.Review => "รอตรวจสอบ",
                    SettlementStatus.Approved => "อนุมัติแล้ว",
                    SettlementStatus.Paid => "โอนสำเร็จ",
                    SettlementStatus.Cancelled => "ยกเลิก",
                    _ => s.Status.ToString()
                };
                var paidDate = s.PaidAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
                var refNo = EscapeCsv(s.TransferReference ?? "-");

                sb.AppendLine($"{s.Id},{ownerName},{phone},{bank},{acc},{s.PeriodStart:yyyy-MM-dd},{s.PeriodEnd:yyyy-MM-dd},{s.GrossRentCollected:F2},{s.ExpensesDeducted:F2},{s.ManagementFeesDeducted:F2},{s.NetPayable:F2},{status},{paidDate},{refNo}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv; charset=utf-8", $"owner-statements-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        });

        // GET /api/v1/reports/agency-cash/export-csv
        group.MapGet("/agency-cash/export-csv", async ([FromServices] IRoomfolioDbContext db, CancellationToken ct) =>
        {
            var payments = await db.Payments
                .Include(p => p.PayerTenant)
                .Include(p => p.Allocations)
                    .ThenInclude(a => a.RentCharge)
                        .ThenInclude(c => c!.Lease)
                            .ThenInclude(l => l!.Unit)
                                .ThenInclude(u => u!.Project)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync(ct);

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine("รหัสการชำระ,วันที่รับชำระ,โครงการ,เลขห้อง,ชื่อผู้ชำระ,วิธีชำระเงิน,เลขที่อ้างอิง,ผู้รับเงิน,จำนวนเงิน (บาท),หมายเหตุ");

            foreach (var p in payments)
            {
                var firstAlloc = p.Allocations.FirstOrDefault();
                var unit = firstAlloc?.RentCharge?.Lease?.Unit;
                var projName = EscapeCsv(unit?.Project?.Name ?? "-");
                var unitNum = EscapeCsv(unit?.UnitNumber ?? "-");
                var payer = EscapeCsv(p.PayerTenant?.FullName ?? "-");
                var refNo = EscapeCsv(p.ReferenceNo ?? "-");
                var method = EscapeCsv(p.Method.ToString());
                var recipient = p.ReceivedByParty == PaymentParty.Agency ? "เอเจนซี่รับแทน (Agency Held)" : "ผู้เช่าโอนตรงเข้าเจ้าของ (Direct to Owner)";
                var note = EscapeCsv(p.Note ?? "-");

                sb.AppendLine($"{p.Id},{p.PaymentDate:yyyy-MM-dd},{projName},{unitNum},{payer},{method},{refNo},{recipient},{p.Amount:F2},{note}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv; charset=utf-8", $"agency-cash-held-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        });

        // GET /api/v1/reports/rent-due/export-csv
        group.MapGet("/rent-due/export-csv", async ([FromServices] IRoomfolioDbContext db, CancellationToken ct) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var charges = await db.RentCharges
                .Include(c => c.Lease)
                    .ThenInclude(l => l!.Unit)
                        .ThenInclude(u => u!.Project)
                .Include(c => c.Lease)
                    .ThenInclude(l => l!.Tenant)
                .Include(c => c.Allocations)
                .OrderBy(c => c.DueDate)
                .ToListAsync(ct);

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine("รหัสใบเรียกเก็บ,ประเภท,งวดเริ่ม,งวดสิ้นสุด,กำหนดชำระ,เลขห้อง,โครงการ,ชื่อผู้เช่า,ยอดเรียกเก็บ (บาท),ยอดชำระแล้ว (บาท),ยอดค้างชำระ (บาท),สถานะ,จำนวนวันเลยกำหนด");

            foreach (var c in charges)
            {
                var lease = c.Lease;
                var unitNum = EscapeCsv(lease?.Unit?.UnitNumber ?? "-");
                var projName = EscapeCsv(lease?.Unit?.Project?.Name ?? "-");
                var tenantName = EscapeCsv(lease?.Tenant?.FullName ?? "-");
                var paidAmount = c.Allocations.Sum(a => a.AllocatedAmount);
                var balance = Math.Max(0, c.Amount - paidAmount);
                var daysOverdue = c.DueDate < today && balance > 0 ? (today.DayNumber - c.DueDate.DayNumber) : 0;
                var status = c.Status switch
                {
                    RentChargeStatus.Paid => "ชำระครบแล้ว",
                    RentChargeStatus.PartiallyPaid => "ชำระบางส่วน",
                    RentChargeStatus.Due => daysOverdue > 0 ? "เกินกำหนดชำระ" : "รอชำระ",
                    RentChargeStatus.Overdue => "เกินกำหนดชำระ",
                    RentChargeStatus.Cancelled => "ยกเลิก",
                    _ => c.Status.ToString()
                };

                sb.AppendLine($"{c.Id},{EscapeCsv(c.ChargeType)},{c.PeriodStart:yyyy-MM-dd},{c.PeriodEnd:yyyy-MM-dd},{c.DueDate:yyyy-MM-dd},{unitNum},{projName},{tenantName},{c.Amount:F2},{paidAmount:F2},{balance:F2},{status},{daysOverdue}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv; charset=utf-8", $"rent-due-outstanding-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        });

        // GET /api/v1/reports/lease-expiry/export-csv
        group.MapGet("/lease-expiry/export-csv", async ([FromServices] IRoomfolioDbContext db, CancellationToken ct) =>
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
            var in90Days = today.AddDays(90);

            var leases = await db.Leases
                .Include(l => l.Unit)
                    .ThenInclude(u => u!.Project)
                .Include(l => l.Tenant)
                .Include(l => l.Unit)
                    .ThenInclude(u => u!.Owner)
                .Where(l => l.Status == LeaseStatus.Active && l.EndDate <= in90Days)
                .OrderBy(l => l.EndDate)
                .ToListAsync(ct);

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine("รหัสสัญญา,เลขห้อง,โครงการ,เจ้าของห้อง,ชื่อผู้เช่า,เบอร์โทรผู้เช่า,วันเริ่มสัญญา,วันสิ้นสุดสัญญา,วันที่เหลือ (วัน),ค่าเช่า/เดือน (บาท),เงินประกัน (บาท),สถานะการเตือน");

            foreach (var l in leases)
            {
                var daysRemaining = l.EndDate.DayNumber - today.DayNumber;
                var urgency = daysRemaining <= 30 ? "ด่วนมาก (ภายใน 30 วัน)" : (daysRemaining <= 60 ? "ต้องเตรียมการ (31-60 วัน)" : "เตือนล่วงหน้า (61-90 วัน)");
                var unitNum = EscapeCsv(l.Unit?.UnitNumber ?? "-");
                var projName = EscapeCsv(l.Unit?.Project?.Name ?? "-");
                var ownerName = EscapeCsv(l.Unit?.Owner?.FullName ?? "-");
                var tenantName = EscapeCsv(l.Tenant?.FullName ?? "-");
                var tenantPhone = EscapeCsv(l.Tenant?.Phone ?? "-");

                sb.AppendLine($"{l.Id},{unitNum},{projName},{ownerName},{tenantName},{tenantPhone},{l.StartDate:yyyy-MM-dd},{l.EndDate:yyyy-MM-dd},{daysRemaining},{l.MonthlyRent:F2},{l.DepositAmount:F2},{urgency}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv; charset=utf-8", $"lease-expiry-90days-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        });

        // GET /api/v1/reports/consolidated/export-csv
        group.MapGet("/consolidated/export-csv", async ([FromServices] IRoomfolioDbContext db, CancellationToken ct) =>
        {
            var units = await db.Units
                .Include(u => u.Project)
                .Include(u => u.Owner)
                .Include(u => u.Leases.Where(l => l.Status == LeaseStatus.Active))
                    .ThenInclude(l => l.Tenant)
                .OrderBy(u => u.Project != null ? u.Project.Name : "")
                .ThenBy(u => u.UnitNumber)
                .ToListAsync(ct);

            var sb = new StringBuilder();
            sb.Append('\uFEFF');
            sb.AppendLine("รายงานภาพรวมพอร์ตโฟลิโอ Roomfolio Master Portfolio Summary");
            sb.AppendLine($"วันที่ออกรายงาน,{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            sb.AppendLine();
            sb.AppendLine("โครงการ,เลขห้อง,ชั้น,ขนาด (ตร.ม.),เจ้าของห้อง,เบอร์เจ้าของ,สถานะห้อง,ผู้เช่าปัจจุบัน,ค่าเช่า/เดือน (บาท),วันหมดสัญญา,เงินประกัน (บาท)");

            foreach (var u in units)
            {
                var proj = EscapeCsv(u.Project?.Name ?? "-");
                var unitNo = EscapeCsv(u.UnitNumber);
                var floor = u.Floor.ToString();
                var area = u.AreaSqM.ToString("F1");
                var owner = EscapeCsv(u.Owner?.FullName ?? "-");
                var phone = EscapeCsv(u.Owner?.Phone ?? "-");
                var status = u.Status switch
                {
                    UnitStatus.Available => "ห้องว่าง (Available)",
                    UnitStatus.Rented => "มีผู้เช่า (Rented)",
                    UnitStatus.UnderMaintenance => "ซ่อมบำรุง (Under Maintenance)",
                    UnitStatus.Reserved => "จองแล้ว (Reserved)",
                    _ => u.Status.ToString()
                };

                var activeLease = u.Leases.FirstOrDefault();
                var tenant = EscapeCsv(activeLease?.Tenant?.FullName ?? "-");
                var rent = activeLease != null ? activeLease.MonthlyRent.ToString("F2") : u.TargetRentPrice.ToString("F2");
                var endDate = activeLease != null ? activeLease.EndDate.ToString("yyyy-MM-dd") : "-";
                var deposit = activeLease != null ? activeLease.DepositAmount.ToString("F2") : "-";

                sb.AppendLine($"{proj},{unitNo},{floor},{area},{owner},{phone},{status},{tenant},{rent},{endDate},{deposit}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return Results.File(bytes, "text/csv; charset=utf-8", $"roomfolio-master-export-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        });

        return app;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        if (value.Contains(',') || value.Contains('\"') || value.Contains('\n') || value.Contains('\r'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }
}
