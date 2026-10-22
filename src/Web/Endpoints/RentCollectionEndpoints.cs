using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Application.Finance;
using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Web.Endpoints;

public static class RentCollectionEndpoints
{
    public static IEndpointRouteBuilder MapRentCollectionEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Rent Collection & Payments");

        // GET /api/v1/rent-charges
        group.MapGet("/rent-charges", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            [FromQuery] int? paymentMode,
            [FromQuery] int? status,
            CancellationToken ct) =>
        {
            var query = db.RentCharges
                .Include(r => r.Lease)
                    .ThenInclude(l => l!.Unit)
                        .ThenInclude(u => u!.Project)
                .Include(r => r.Lease)
                    .ThenInclude(l => l!.Tenant)
                .Include(r => r.Allocations)
                    .ThenInclude(a => a.Payment)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(r =>
                    (r.Description != null && r.Description.ToLower().Contains(term)) ||
                    (r.Lease != null && r.Lease.LeaseNumber.ToLower().Contains(term)) ||
                    (r.Lease != null && r.Lease.Unit != null && r.Lease.Unit.UnitNumber.ToLower().Contains(term)) ||
                    (r.Lease != null && r.Lease.Unit != null && r.Lease.Unit.Project != null && r.Lease.Unit.Project.Name.ToLower().Contains(term)) ||
                    (r.Lease != null && r.Lease.Tenant != null && r.Lease.Tenant.FullName.ToLower().Contains(term)));
            }

            if (paymentMode.HasValue && Enum.IsDefined(typeof(RentalPaymentMode), paymentMode.Value))
            {
                var mode = (RentalPaymentMode)paymentMode.Value;
                query = query.Where(r => r.Lease != null && r.Lease.PaymentMode == mode);
            }

            if (status.HasValue && Enum.IsDefined(typeof(RentChargeStatus), status.Value))
            {
                var st = (RentChargeStatus)status.Value;
                query = query.Where(r => r.Status == st);
            }

            var allCharges = await query
                .OrderByDescending(r => r.DueDate)
                .ToListAsync(ct);

            var items = allCharges.Select(r =>
            {
                var paid = r.Allocations.Sum(a => a.AllocatedAmount);
                var outstanding = Math.Max(0, r.Amount - paid);
                var mode = r.Lease?.PaymentMode ?? RentalPaymentMode.DirectToOwner;
                var modeText = mode == RentalPaymentMode.ThroughAgent ? "ผ่านนายหน้า" : "จ่ายตรงเจ้าของ";

                string statusText;
                string statusClass;

                if (outstanding == 0 && paid > 0)
                {
                    if (mode == RentalPaymentMode.ThroughAgent)
                    {
                        statusText = "เงินอยู่นายหน้า";
                        statusClass = "badge-emerald";
                    }
                    else
                    {
                        statusText = "ชำระครบแล้ว";
                        statusClass = "badge-emerald";
                    }
                }
                else if (paid > 0 && outstanding > 0)
                {
                    statusText = "ชำระบางส่วน";
                    statusClass = "badge-amber";
                }
                else
                {
                    var today = DateOnly.FromDateTime(DateTime.UtcNow);
                    if (r.DueDate < today)
                    {
                        statusText = "เกินกำหนด";
                        statusClass = "badge-rose";
                    }
                    else
                    {
                        statusText = "รอชำระ";
                        statusClass = "badge-gray";
                    }
                }

                var periodStr = $"{r.PeriodStart:dd/MM} - {r.PeriodEnd:dd/MM/yyyy}";

                return new RentChargeResponseDto(
                    r.Id,
                    r.LeaseId,
                    r.Lease?.LeaseNumber ?? "-",
                    r.Lease?.Unit?.UnitNumber ?? "-",
                    r.Lease?.Unit?.Project?.Name ?? "-",
                    r.Lease?.Tenant?.FullName ?? "-",
                    r.Lease?.Tenant?.Phone ?? "-",
                    periodStr,
                    r.DueDate,
                    r.Amount,
                    paid,
                    outstanding,
                    mode,
                    modeText,
                    r.Status,
                    statusText,
                    statusClass,
                    r.CreatedAt
                );
            }).ToList();

            // Calculate KPIs
            var totalTarget = items.Sum(i => i.ChargeAmount);
            var totalCollected = items.Sum(i => i.PaidAmount);
            var totalOutstanding = items.Sum(i => i.Outstanding);

            // Cash held by agency: Only allocations where payment mode is ThroughAgent and received by agency
            var agencyCashHeld = allCharges
                .Where(r => r.Lease?.PaymentMode == RentalPaymentMode.ThroughAgent)
                .SelectMany(r => r.Allocations)
                .Where(a => a.Payment?.ReceivedByParty == PaymentParty.Agency)
                .Sum(a => a.AllocatedAmount);

            var summary = new RentCollectionSummaryDto(
                totalTarget,
                totalCollected,
                totalOutstanding,
                agencyCashHeld,
                items.Count,
                items.Count(i => i.Outstanding == 0 && i.PaidAmount > 0),
                items.Count(i => i.Outstanding > 0),
                items
            );

            return Results.Ok(summary);
        })
        .WithName("GetRentCharges")
        .WithOpenApi();

        // GET /api/v1/rent-charges/{id}
        group.MapGet("/rent-charges/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var r = await db.RentCharges
                .Include(r => r.Lease)
                    .ThenInclude(l => l!.Unit)
                        .ThenInclude(u => u!.Project)
                .Include(r => r.Lease)
                    .ThenInclude(l => l!.Tenant)
                .Include(r => r.Allocations)
                    .ThenInclude(a => a.Payment)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (r is null)
                return Results.NotFound(new { message = $"RentCharge with ID {id} not found." });

            var paid = r.Allocations.Sum(a => a.AllocatedAmount);
            var outstanding = Math.Max(0, r.Amount - paid);
            var mode = r.Lease?.PaymentMode ?? RentalPaymentMode.DirectToOwner;
            var modeText = mode == RentalPaymentMode.ThroughAgent ? "ผ่านนายหน้า" : "จ่ายตรงเจ้าของ";

            var periodStr = $"{r.PeriodStart:dd/MM} - {r.PeriodEnd:dd/MM/yyyy}";

            var dto = new RentChargeResponseDto(
                r.Id,
                r.LeaseId,
                r.Lease?.LeaseNumber ?? "-",
                r.Lease?.Unit?.UnitNumber ?? "-",
                r.Lease?.Unit?.Project?.Name ?? "-",
                r.Lease?.Tenant?.FullName ?? "-",
                r.Lease?.Tenant?.Phone ?? "-",
                periodStr,
                r.DueDate,
                r.Amount,
                paid,
                outstanding,
                mode,
                modeText,
                r.Status,
                outstanding == 0 ? "ชำระครบแล้ว" : (paid > 0 ? "ชำระบางส่วน" : "รอชำระ"),
                outstanding == 0 ? "badge-emerald" : "badge-amber",
                r.CreatedAt
            );

            return Results.Ok(dto);
        })
        .WithName("GetRentChargeById")
        .WithOpenApi();

        // POST /api/v1/payments/record (Record Payment & Allocation)
        group.MapPost("/payments/record", async (
            [FromBody] RecordPaymentRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            if (request.Amount <= 0)
                return Results.BadRequest(new { message = "จำนวนเงินที่ชำระต้องมากกว่า 0" });

            var rentCharge = await db.RentCharges
                .Include(r => r.Lease)
                .Include(r => r.Allocations)
                .FirstOrDefaultAsync(r => r.Id == request.RentChargeId, ct);

            if (rentCharge is null)
                return Results.NotFound(new { message = "ไม่พบงวดค่าเช่าที่ระบุ" });

            var existingPaid = rentCharge.Allocations.Sum(a => a.AllocatedAmount);
            var maxReceivable = rentCharge.Amount - existingPaid;

            if (maxReceivable <= 0)
                return Results.BadRequest(new { message = "งวดค่าเช่านี้ชำระเต็มจำนวนแล้ว ไม่มียอดคงค้าง" });

            var payment = new Payment
            {
                PayerPartyType = PaymentParty.Tenant,
                PayerTenantId = rentCharge.Lease?.TenantId,
                ReceivedByParty = request.ReceivedByParty,
                PaymentMode = request.PaymentMode,
                PaymentDate = request.PaymentDate,
                Amount = request.Amount,
                Method = request.Method,
                ReferenceNo = request.ReferenceNo?.Trim(),
                Status = PaymentStatus.Confirmed,
                Note = request.Note?.Trim()
            };

            db.Payments.Add(payment);
            await db.SaveChangesAsync(ct);

            // Create allocation
            var allocAmount = Math.Min(request.Amount, maxReceivable);
            var allocation = new PaymentAllocation
            {
                PaymentId = payment.Id,
                RentChargeId = rentCharge.Id,
                AllocatedAmount = allocAmount,
                Note = $"จัดสรรการชำระ {allocAmount:N2} บาท"
            };

            db.PaymentAllocations.Add(allocation);

            // Update rent charge status
            var newTotalPaid = existingPaid + allocAmount;
            if (newTotalPaid >= rentCharge.Amount)
            {
                rentCharge.Status = RentChargeStatus.Paid;
            }
            else
            {
                rentCharge.Status = RentChargeStatus.PartiallyPaid;
            }

            await db.SaveChangesAsync(ct);

            var methodText = request.Method switch
            {
                PaymentMethod.BankTransfer => "โอนเงินผ่านธนาคาร",
                PaymentMethod.PromptPayQR => "พร้อมเพย์ QR Code",
                PaymentMethod.Cash => "เงินสด",
                _ => "อื่นๆ"
            };

            var receivedByText = request.ReceivedByParty switch
            {
                PaymentParty.Agency => "นายหน้ารับแทน",
                PaymentParty.Owner => "เจ้าของรับโดยตรง",
                _ => "อื่นๆ"
            };

            var responseDto = new PaymentResponseDto(
                payment.Id,
                rentCharge.Id,
                payment.Amount,
                payment.PaymentDate,
                payment.Method,
                methodText,
                payment.PaymentMode,
                payment.PaymentMode == RentalPaymentMode.ThroughAgent ? "ผ่านนายหน้า" : "จ่ายตรงเจ้าของ",
                payment.ReceivedByParty,
                receivedByText,
                payment.ReferenceNo,
                payment.Status,
                payment.Note,
                payment.CreatedAt
            );

            return Results.Ok(responseDto);
        })
        .WithName("RecordPayment")
        .WithOpenApi();

        // DELETE /api/v1/rent-charges/{id}
        group.MapDelete("/rent-charges/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var charge = await db.RentCharges
                .Include(r => r.Allocations)
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (charge is null)
                return Results.NotFound(new { message = $"RentCharge with ID {id} not found." });

            if (charge.Allocations.Count > 0)
            {
                return Results.BadRequest(new { message = "ไม่สามารถลบงวดค่าเช่าที่มีรายการรับชำระเงินผูกอยู่ได้ แนะนำให้ Void รายการรับเงินก่อน" });
            }

            db.RentCharges.Remove(charge);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteRentCharge")
        .WithOpenApi();

        return app;
    }
}
