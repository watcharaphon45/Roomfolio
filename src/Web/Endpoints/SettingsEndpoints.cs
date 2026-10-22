using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Domain.Organizations;

namespace Roomfolio.Web.Endpoints;

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/settings").WithTags("Settings & Organization");

        // GET /api/v1/settings
        group.MapGet("/", async ([FromServices] IRoomfolioDbContext db, CancellationToken ct) =>
        {
            var org = await db.Organizations.FirstOrDefaultAsync(ct);
            if (org == null)
            {
                org = new Organization
                {
                    Name = "Prime Living Agency Co., Ltd.",
                    TaxId = "0105565012345",
                    DefaultCurrency = "THB (฿)",
                    Timezone = "Asia/Bangkok (UTC+07:00)",
                    PlanName = "Pro Plan",
                    PlanPrice = 799,
                    RoomLimit = 200,
                    Address = "88/1 Sukhumvit 55, Thonglor, Watthana, Bangkok 10110",
                    Phone = "02-712-9988",
                    Email = "contact@primeliving.co.th"
                };
                db.Organizations.Add(org);
                await db.SaveChangesAsync(ct);
            }

            var currentRoomCount = await db.Units.CountAsync(ct);

            var response = new OrganizationSettingsResponse(
                org.Id,
                org.Name,
                org.TaxId,
                org.DefaultCurrency,
                org.Timezone,
                org.PlanName,
                org.PlanPrice,
                currentRoomCount,
                org.RoomLimit,
                org.Address,
                org.Phone,
                org.Email,
                org.UpdatedAt ?? org.CreatedAt
            );

            return Results.Ok(response);
        });

        // PUT /api/v1/settings
        group.MapPut("/", async (
            [FromServices] IRoomfolioDbContext db,
            [FromBody] UpdateOrganizationSettingsRequest request,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName))
            {
                return Results.BadRequest(new { message = "Organization name is required." });
            }

            var org = await db.Organizations.FirstOrDefaultAsync(ct);
            if (org == null)
            {
                org = new Organization();
                db.Organizations.Add(org);
            }

            org.Name = request.CompanyName.Trim();
            if (request.TaxId != null) org.TaxId = request.TaxId.Trim();
            if (request.DefaultCurrency != null) org.DefaultCurrency = request.DefaultCurrency.Trim();
            if (request.Timezone != null) org.Timezone = request.Timezone.Trim();
            if (request.Address != null) org.Address = request.Address.Trim();
            if (request.Phone != null) org.Phone = request.Phone.Trim();
            if (request.Email != null) org.Email = request.Email.Trim();

            await db.SaveChangesAsync(ct);

            var currentRoomCount = await db.Units.CountAsync(ct);

            var response = new OrganizationSettingsResponse(
                org.Id,
                org.Name,
                org.TaxId,
                org.DefaultCurrency,
                org.Timezone,
                org.PlanName,
                org.PlanPrice,
                currentRoomCount,
                org.RoomLimit,
                org.Address,
                org.Phone,
                org.Email,
                org.UpdatedAt ?? org.CreatedAt
            );

            return Results.Ok(response);
        });

        // POST /api/v1/settings/upgrade-plan
        group.MapPost("/upgrade-plan", async (
            [FromServices] IRoomfolioDbContext db,
            [FromBody] UpgradePlanRequest request,
            CancellationToken ct) =>
        {
            var org = await db.Organizations.FirstOrDefaultAsync(ct);
            if (org == null)
            {
                org = new Organization();
                db.Organizations.Add(org);
            }

            if (!string.IsNullOrWhiteSpace(request.PlanName))
            {
                org.PlanName = request.PlanName.Trim();
            }
            if (request.PlanPrice > 0)
            {
                org.PlanPrice = request.PlanPrice;
            }
            if (request.RoomLimit > 0)
            {
                org.RoomLimit = request.RoomLimit;
            }

            await db.SaveChangesAsync(ct);

            var currentRoomCount = await db.Units.CountAsync(ct);

            var response = new OrganizationSettingsResponse(
                org.Id,
                org.Name,
                org.TaxId,
                org.DefaultCurrency,
                org.Timezone,
                org.PlanName,
                org.PlanPrice,
                currentRoomCount,
                org.RoomLimit,
                org.Address,
                org.Phone,
                org.Email,
                org.UpdatedAt ?? org.CreatedAt
            );

            return Results.Ok(response);
        });

        return app;
    }
}

public record OrganizationSettingsResponse(
    Guid Id,
    string CompanyName,
    string TaxId,
    string DefaultCurrency,
    string Timezone,
    string PlanName,
    decimal PlanPrice,
    int RoomCount,
    int RoomLimit,
    string? Address,
    string? Phone,
    string? Email,
    DateTimeOffset UpdatedAt
);

public record UpdateOrganizationSettingsRequest(
    string CompanyName,
    string? TaxId,
    string? DefaultCurrency,
    string? Timezone,
    string? Address,
    string? Phone,
    string? Email
);

public record UpgradePlanRequest(
    string PlanName,
    decimal PlanPrice,
    int RoomLimit
);
