using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Application.Finance;

public record RentCollectionSummaryDto(
    decimal TotalTarget,
    decimal TotalCollected,
    decimal TotalOutstanding,
    decimal AgencyCashHeld,
    int TotalChargesCount,
    int PaidChargesCount,
    int PendingChargesCount,
    IReadOnlyList<RentChargeResponseDto> Items
);

public record RentChargeResponseDto(
    Guid Id,
    Guid LeaseId,
    string LeaseNumber,
    string UnitNumber,
    string ProjectName,
    string TenantName,
    string TenantPhone,
    string Period,
    DateOnly DueDate,
    decimal ChargeAmount,
    decimal PaidAmount,
    decimal Outstanding,
    RentalPaymentMode PaymentMode,
    string PaymentModeText,
    RentChargeStatus Status,
    string StatusText,
    string StatusClass,
    DateTimeOffset CreatedAt
);

public record CreateRentChargeRequest(
    Guid LeaseId,
    string ChargeType,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly DueDate,
    string? Description,
    decimal Amount
);
