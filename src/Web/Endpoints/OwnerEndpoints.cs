using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Application.Properties;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Web.Endpoints;

public static class OwnerEndpoints
{
    public static IEndpointRouteBuilder MapOwnerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Owners & Landlords");

        // GET /api/v1/owners
        group.MapGet("/owners", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            CancellationToken ct) =>
        {
            var query = db.Owners
                .Include(o => o.Units)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(o =>
                    o.FullName.ToLower().Contains(term) ||
                    o.Phone.Contains(term) ||
                    (o.Email != null && o.Email.ToLower().Contains(term)) ||
                    (o.LineId != null && o.LineId.ToLower().Contains(term)) ||
                    (o.BankName != null && o.BankName.ToLower().Contains(term)) ||
                    o.Units.Any(u => u.UnitNumber.ToLower().Contains(term)));
            }

            var owners = await query
                .OrderBy(o => o.FullName)
                .Select(o => new OwnerResponseDto(
                    o.Id,
                    o.FullName,
                    o.Phone,
                    o.Email,
                    o.BankName,
                    o.BankAccountNumber,
                    o.LineId,
                    o.Units.Count,
                    o.Units.Select(u => u.UnitNumber).ToList(),
                    o.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(owners);
        })
        .WithName("GetOwners")
        .WithOpenApi();

        // GET /api/v1/owners/{id}
        group.MapGet("/owners/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var owner = await db.Owners
                .Include(o => o.Units)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            if (owner is null)
                return Results.NotFound(new { message = $"Owner with ID {id} not found." });

            var dto = new OwnerResponseDto(
                owner.Id,
                owner.FullName,
                owner.Phone,
                owner.Email,
                owner.BankName,
                owner.BankAccountNumber,
                owner.LineId,
                owner.Units.Count,
                owner.Units.Select(u => u.UnitNumber).ToList(),
                owner.CreatedAt
            );

            return Results.Ok(dto);
        })
        .WithName("GetOwnerById")
        .WithOpenApi();

        // POST /api/v1/owners (Create)
        group.MapPost("/owners", async (
            [FromBody] CreateOwnerRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone))
                return Results.BadRequest(new { message = "ชื่อ-นามสกุล และเบอร์โทรศัพท์ เป็นข้อมูลจำเป็น" });

            var owner = new Owner
            {
                FullName = request.FullName.Trim(),
                Phone = request.Phone.Trim(),
                Email = request.Email?.Trim(),
                BankName = request.BankName?.Trim(),
                BankAccountNumber = request.BankAccountNumber?.Trim(),
                LineId = request.LineId?.Trim()
            };

            db.Owners.Add(owner);
            await db.SaveChangesAsync(ct);

            var responseDto = new OwnerResponseDto(
                owner.Id,
                owner.FullName,
                owner.Phone,
                owner.Email,
                owner.BankName,
                owner.BankAccountNumber,
                owner.LineId,
                0,
                Array.Empty<string>(),
                owner.CreatedAt
            );

            return Results.Created($"/api/v1/owners/{owner.Id}", responseDto);
        })
        .WithName("CreateOwner")
        .WithOpenApi();

        // PUT /api/v1/owners/{id} (Update)
        group.MapPut("/owners/{id:guid}", async (
            Guid id,
            [FromBody] UpdateOwnerRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var owner = await db.Owners.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (owner is null)
                return Results.NotFound(new { message = $"Owner with ID {id} not found." });

            if (string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Phone))
                return Results.BadRequest(new { message = "ชื่อ-นามสกุล และเบอร์โทรศัพท์ เป็นข้อมูลจำเป็น" });

            owner.FullName = request.FullName.Trim();
            owner.Phone = request.Phone.Trim();
            owner.Email = request.Email?.Trim();
            owner.BankName = request.BankName?.Trim();
            owner.BankAccountNumber = request.BankAccountNumber?.Trim();
            owner.LineId = request.LineId?.Trim();

            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UpdateOwner")
        .WithOpenApi();

        // DELETE /api/v1/owners/{id} (Delete)
        group.MapDelete("/owners/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var owner = await db.Owners.FirstOrDefaultAsync(o => o.Id == id, ct);
            if (owner is null)
                return Results.NotFound(new { message = $"Owner with ID {id} not found." });

            db.Owners.Remove(owner);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteOwner")
        .WithOpenApi();

        return app;
    }
}
