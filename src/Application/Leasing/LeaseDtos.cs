using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Application.Leasing;

public record LeaseResponseDto(
    Guid Id,
    Guid UnitId,
    string UnitNumber,
    string ProjectName,
    Guid TenantId,
    string TenantName,
    string TenantPhone,
    string LeaseNumber,
    DateOnly StartDate,
    DateOnly EndDate,
    int RentDueDay,
    decimal MonthlyRent,
    RentalPaymentMode PaymentMode,
    string PaymentModeText,
    decimal DepositAmount,
    DepositHolder DepositHolder,
    string DepositHolderText,
    LeaseStatus Status,
    string StatusText,
    DateOnly? SignedAt,
    string? Note,
    DateTimeOffset CreatedAt
);

public record CreateLeaseRequest(
    Guid UnitId,
    Guid TenantId,
    string LeaseNumber,
    DateOnly StartDate,
    DateOnly EndDate,
    int RentDueDay,
    decimal MonthlyRent,
    RentalPaymentMode PaymentMode,
    decimal DepositAmount,
    DepositHolder DepositHolder,
    LeaseStatus Status,
    DateOnly? SignedAt,
    string? Note
);

public record UpdateLeaseRequest(
    Guid UnitId,
    Guid TenantId,
    string LeaseNumber,
    DateOnly StartDate,
    DateOnly EndDate,
    int RentDueDay,
    decimal MonthlyRent,
    RentalPaymentMode PaymentMode,
    decimal DepositAmount,
    DepositHolder DepositHolder,
    LeaseStatus Status,
    DateOnly? SignedAt,
    DateOnly? TerminatedAt,
    string? TerminationReason,
    string? Note
);
