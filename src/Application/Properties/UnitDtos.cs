using Roomfolio.Domain.Properties;

namespace Roomfolio.Application.Properties;

public record UnitResponseDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    Guid? OwnerId,
    string? OwnerName,
    string? OwnerPhone,
    string UnitNumber,
    int Floor,
    string? Building,
    int Bedrooms,
    int Bathrooms,
    decimal AreaSqM,
    decimal TargetRentPrice,
    UnitStatus Status,
    string StatusText,
    RentalPaymentMode PaymentMode,
    string PaymentModeText,
    string? TenantName,
    string? Note,
    DateTimeOffset CreatedAt
);

public record CreateUnitRequest(
    Guid ProjectId,
    Guid? OwnerId,
    string UnitNumber,
    int Floor,
    string? Building,
    int Bedrooms,
    int Bathrooms,
    decimal AreaSqM,
    decimal TargetRentPrice,
    UnitStatus Status,
    RentalPaymentMode PaymentMode,
    string? TenantName,
    string? Note
);

public record UpdateUnitRequest(
    Guid ProjectId,
    Guid? OwnerId,
    string UnitNumber,
    int Floor,
    string? Building,
    int Bedrooms,
    int Bathrooms,
    decimal AreaSqM,
    decimal TargetRentPrice,
    UnitStatus Status,
    RentalPaymentMode PaymentMode,
    string? TenantName,
    string? Note
);

public record ProjectLookupDto(Guid Id, string Name);
public record OwnerLookupDto(Guid Id, string FullName, string Phone);
