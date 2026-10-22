using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Domain.Finance;

namespace Roomfolio.Web.Endpoints;

public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/expenses").WithTags("Expenses & Maintenance");

        // GET /api/v1/expenses
        group.MapGet("/", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            [FromQuery] int? category,
            [FromQuery] bool? deductible,
            CancellationToken ct) =>
        {
            var query = db.Expenses
                .Include(e => e.Unit)
                    .ThenInclude(u => u!.Project)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(e =>
                    e.Description.ToLower().Contains(term) ||
                    (e.Unit != null && e.Unit.UnitNumber.ToLower().Contains(term)) ||
                    (e.Unit != null && e.Unit.Project != null && e.Unit.Project.Name.ToLower().Contains(term)));
            }

            if (category.HasValue && Enum.IsDefined(typeof(ExpenseCategory), category.Value))
            {
                var cat = (ExpenseCategory)category.Value;
                query = query.Where(e => e.Category == cat);
            }

            if (deductible.HasValue)
            {
                query = query.Where(e => e.IsDeductibleFromOwner == deductible.Value);
            }

            var expenses = await query
                .OrderByDescending(e => e.IncurredDate)
                .ToListAsync(ct);

            var items = expenses.Select(e =>
            {
                var categoryName = e.Category switch
                {
                    ExpenseCategory.Cleaning => "ทำความสะอาด",
                    ExpenseCategory.Repair => "ซ่อมแซม/อุปกรณ์",
                    ExpenseCategory.Keycard => "กุญแจ/คีย์การ์ด",
                    ExpenseCategory.Aircon => "ล้างแอร์/บำรุงรักษา",
                    ExpenseCategory.CommonFee => "ค่าส่วนกลาง",
                    _ => "อื่นๆ"
                };

                var paidByName = e.PaidBy switch
                {
                    ExpensePaidBy.Agency => "Agency (สำรองจ่าย)",
                    ExpensePaidBy.Owner => "Owner (จ่ายเอง)",
                    ExpensePaidBy.Tenant => "Tenant (ผู้เช่าจ่าย)",
                    _ => "อื่นๆ"
                };

                var statusName = e.Status switch
                {
                    ExpenseStatus.Confirmed => "ยืนยันแล้ว",
                    ExpenseStatus.Settled => "หักรอบโอนแล้ว",
                    ExpenseStatus.Voided => "ยกเลิก",
                    _ => "รอยืนยัน"
                };

                var unitDisplay = e.Unit != null
                    ? $"{e.Unit.Project?.Name ?? "โครงการ"} {e.Unit.UnitNumber}"
                    : "ไม่ระบุห้อง";

                return new
                {
                    id = e.Id.ToString(),
                    unitId = e.UnitId.ToString(),
                    unit = unitDisplay,
                    date = e.IncurredDate.ToString("dd MMM yyyy"),
                    rawDate = e.IncurredDate.ToString("yyyy-MM-dd"),
                    categoryId = (int)e.Category,
                    category = categoryName,
                    description = e.Description,
                    paidById = (int)e.PaidBy,
                    paidBy = paidByName,
                    amount = e.Amount,
                    deductible = e.IsDeductibleFromOwner,
                    status = statusName,
                    receiptUrl = e.ReceiptUrl,
                    note = e.Note
                };
            }).ToList();

            var totalAmount = items.Sum(i => i.amount);
            var deductibleAmount = items.Where(i => i.deductible).Sum(i => i.amount);

            return Results.Ok(new
            {
                totalCount = items.Count,
                totalAmount,
                deductibleAmount,
                items
            });
        })
        .WithName("GetExpenses")
        .WithOpenApi();

        // POST /api/v1/expenses
        group.MapPost("/", async (
            [FromServices] IRoomfolioDbContext db,
            [FromBody] CreateExpenseRequest req,
            CancellationToken ct) =>
        {
            if (req.Amount <= 0)
            {
                return Results.BadRequest(new { error = "จำนวนเงินต้องมากกว่า 0" });
            }

            if (string.IsNullOrWhiteSpace(req.Description))
            {
                return Results.BadRequest(new { error = "กรุณาระบุรายละเอียดค่าใช้จ่าย" });
            }

            var unit = await db.Units.FindAsync(new object[] { req.UnitId }, ct);
            if (unit == null)
            {
                return Results.BadRequest(new { error = "ไม่พบห้องชุดที่ระบุ" });
            }

            var incurred = req.IncurredDate.HasValue 
                ? req.IncurredDate.Value 
                : DateTimeOffset.UtcNow;

            var expense = new Expense
            {
                UnitId = req.UnitId,
                Category = (ExpenseCategory)req.Category,
                Description = req.Description.Trim(),
                Amount = req.Amount,
                IncurredDate = incurred,
                PaidBy = (ExpensePaidBy)req.PaidBy,
                IsDeductibleFromOwner = req.IsDeductibleFromOwner,
                Status = ExpenseStatus.Confirmed,
                ReceiptUrl = req.ReceiptUrl?.Trim(),
                Note = req.Note?.Trim()
            };

            db.Expenses.Add(expense);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/expenses/{expense.Id}", new { id = expense.Id });
        })
        .WithName("CreateExpense")
        .WithOpenApi();

        // DELETE /api/v1/expenses/{id}
        group.MapDelete("/{id:guid}", async (
            [FromRoute] Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var expense = await db.Expenses.FindAsync(new object[] { id }, ct);
            if (expense == null)
            {
                return Results.NotFound(new { error = "ไม่พบรายการค่าใช้จ่าย" });
            }

            db.Expenses.Remove(expense);
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { success = true });
        })
        .WithName("DeleteExpense")
        .WithOpenApi();

        return app;
    }
}

public record CreateExpenseRequest(
    Guid UnitId,
    int Category,
    string Description,
    decimal Amount,
    DateTimeOffset? IncurredDate,
    int PaidBy,
    bool IsDeductibleFromOwner,
    string? ReceiptUrl,
    string? Note
);
