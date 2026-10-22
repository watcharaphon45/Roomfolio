using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Application.Properties;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Web.Endpoints;

public static class UnitEndpoints
{
    public static IEndpointRouteBuilder MapUnitEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1").WithTags("Units & Properties");

        // GET /api/v1/units
        group.MapGet("/units", async (
            [FromServices] IRoomfolioDbContext db,
            [FromQuery] string? search,
            [FromQuery] UnitStatus? status,
            CancellationToken ct) =>
        {
            var query = db.Units
                .Include(u => u.Project)
                .Include(u => u.Owner)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    u.UnitNumber.ToLower().Contains(term) ||
                    (u.Project != null && u.Project.Name.ToLower().Contains(term)) ||
                    (u.TenantName != null && u.TenantName.ToLower().Contains(term)) ||
                    (u.Owner != null && u.Owner.FullName.ToLower().Contains(term)));
            }

            if (status.HasValue)
            {
                query = query.Where(u => u.Status == status.Value);
            }

            var units = await query
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UnitResponseDto(
                    u.Id,
                    u.ProjectId,
                    u.Project != null ? u.Project.Name : string.Empty,
                    u.OwnerId,
                    u.Owner != null ? u.Owner.FullName : null,
                    u.Owner != null ? u.Owner.Phone : null,
                    u.UnitNumber,
                    u.Floor,
                    u.Building,
                    u.Bedrooms,
                    u.Bathrooms,
                    u.AreaSqM,
                    u.TargetRentPrice,
                    u.Status,
                    GetStatusText(u.Status),
                    u.PaymentMode,
                    u.PaymentMode == RentalPaymentMode.DirectToOwner ? "จ่ายตรงเจ้าของ" : "ผ่านนายหน้า",
                    u.TenantName,
                    u.Note,
                    u.CreatedAt
                ))
                .ToListAsync(ct);

            return Results.Ok(units);
        })
        .WithName("GetUnits")
        .WithOpenApi();

        // GET /api/v1/units/{id}
        group.MapGet("/units/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var unit = await db.Units
                .Include(u => u.Project)
                .Include(u => u.Owner)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id, ct);

            if (unit is null)
                return Results.NotFound(new { message = $"Unit with ID {id} not found." });

            var dto = new UnitResponseDto(
                unit.Id,
                unit.ProjectId,
                unit.Project?.Name ?? string.Empty,
                unit.OwnerId,
                unit.Owner?.FullName,
                unit.Owner?.Phone,
                unit.UnitNumber,
                unit.Floor,
                unit.Building,
                unit.Bedrooms,
                unit.Bathrooms,
                unit.AreaSqM,
                unit.TargetRentPrice,
                unit.Status,
                GetStatusText(unit.Status),
                unit.PaymentMode,
                unit.PaymentMode == RentalPaymentMode.DirectToOwner ? "จ่ายตรงเจ้าของ" : "ผ่านนายหน้า",
                unit.TenantName,
                unit.Note,
                unit.CreatedAt
            );

            return Results.Ok(dto);
        })
        .WithName("GetUnitById")
        .WithOpenApi();

        // POST /api/v1/units (Create)
        group.MapPost("/units", async (
            [FromBody] CreateUnitRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var projectExists = await db.Projects.AnyAsync(p => p.Id == request.ProjectId, ct);
            if (!projectExists)
                return Results.BadRequest(new { message = "ProjectId does not exist." });

            if (request.OwnerId.HasValue)
            {
                var ownerExists = await db.Owners.AnyAsync(o => o.Id == request.OwnerId.Value, ct);
                if (!ownerExists)
                    return Results.BadRequest(new { message = "OwnerId does not exist." });
            }

            var unit = new Unit
            {
                ProjectId = request.ProjectId,
                OwnerId = request.OwnerId,
                UnitNumber = request.UnitNumber.Trim(),
                Floor = request.Floor,
                Building = request.Building?.Trim(),
                Bedrooms = request.Bedrooms,
                Bathrooms = request.Bathrooms,
                AreaSqM = request.AreaSqM,
                TargetRentPrice = request.TargetRentPrice,
                Status = request.Status,
                PaymentMode = request.PaymentMode,
                TenantName = request.TenantName?.Trim(),
                Note = request.Note?.Trim()
            };

            db.Units.Add(unit);
            await db.SaveChangesAsync(ct);

            var createdProject = await db.Projects.FindAsync([unit.ProjectId], ct);
            var createdOwner = unit.OwnerId.HasValue ? await db.Owners.FindAsync([unit.OwnerId.Value], ct) : null;

            var responseDto = new UnitResponseDto(
                unit.Id,
                unit.ProjectId,
                createdProject?.Name ?? string.Empty,
                unit.OwnerId,
                createdOwner?.FullName,
                createdOwner?.Phone,
                unit.UnitNumber,
                unit.Floor,
                unit.Building,
                unit.Bedrooms,
                unit.Bathrooms,
                unit.AreaSqM,
                unit.TargetRentPrice,
                unit.Status,
                GetStatusText(unit.Status),
                unit.PaymentMode,
                unit.PaymentMode == RentalPaymentMode.DirectToOwner ? "จ่ายตรงเจ้าของ" : "ผ่านนายหน้า",
                unit.TenantName,
                unit.Note,
                unit.CreatedAt
            );

            return Results.Created($"/api/v1/units/{unit.Id}", responseDto);
        })
        .WithName("CreateUnit")
        .WithOpenApi();

        // PUT /api/v1/units/{id} (Update)
        group.MapPut("/units/{id:guid}", async (
            Guid id,
            [FromBody] UpdateUnitRequest request,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var unit = await db.Units.FirstOrDefaultAsync(u => u.Id == id, ct);
            if (unit is null)
                return Results.NotFound(new { message = $"Unit with ID {id} not found." });

            var projectExists = await db.Projects.AnyAsync(p => p.Id == request.ProjectId, ct);
            if (!projectExists)
                return Results.BadRequest(new { message = "ProjectId does not exist." });

            if (request.OwnerId.HasValue)
            {
                var ownerExists = await db.Owners.AnyAsync(o => o.Id == request.OwnerId.Value, ct);
                if (!ownerExists)
                    return Results.BadRequest(new { message = "OwnerId does not exist." });
            }

            unit.ProjectId = request.ProjectId;
            unit.OwnerId = request.OwnerId;
            unit.UnitNumber = request.UnitNumber.Trim();
            unit.Floor = request.Floor;
            unit.Building = request.Building?.Trim();
            unit.Bedrooms = request.Bedrooms;
            unit.Bathrooms = request.Bathrooms;
            unit.AreaSqM = request.AreaSqM;
            unit.TargetRentPrice = request.TargetRentPrice;
            unit.Status = request.Status;
            unit.PaymentMode = request.PaymentMode;
            unit.TenantName = request.TenantName?.Trim();
            unit.Note = request.Note?.Trim();

            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("UpdateUnit")
        .WithOpenApi();

        // DELETE /api/v1/units/{id} (Delete)
        group.MapDelete("/units/{id:guid}", async (
            Guid id,
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var unit = await db.Units.FirstOrDefaultAsync(u => u.Id == id, ct);
            if (unit is null)
                return Results.NotFound(new { message = $"Unit with ID {id} not found." });

            db.Units.Remove(unit);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        })
        .WithName("DeleteUnit")
        .WithOpenApi();

        // GET /api/v1/projects (Dropdown lookup)
        group.MapGet("/projects", async (
            [FromServices] IRoomfolioDbContext db,
            CancellationToken ct) =>
        {
            var projects = await db.Projects
                .AsNoTracking()
                .OrderBy(p => p.Name)
                .Select(p => new ProjectLookupDto(p.Id, p.Name))
                .ToListAsync(ct);

            return Results.Ok(projects);
        })
        .WithName("GetProjectLookups")
        .WithOpenApi();

        return app;
    }

    private static string GetStatusText(UnitStatus status) => status switch
    {
        UnitStatus.Available => "ห้องว่าง",
        UnitStatus.Rented => "ติดสัญญาเช่า",
        UnitStatus.UnderMaintenance => "ซ่อมบำรุง",
        UnitStatus.Reserved => "จองแล้ว",
        _ => "ไม่ระบุ"
    };
}
