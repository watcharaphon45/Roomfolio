using Roomfolio.Domain.Finance;

namespace Roomfolio.Application.Finance;

public record SettlementPipelineSummaryDto(
    int DraftCount,
    int ReviewCount,
    int ApprovedCount,
    int PaidCount,
    decimal TotalNetPayable,
    IReadOnlyList<OwnerSettlementResponseDto> Settlements
);

public record SettlementLineDto(
    Guid Id,
    SettlementLineType LineType,
    string LineTypeText,
    Guid? UnitId,
    string? UnitNumber,
    string Description,
    decimal Amount,
    string? Note
);

public record OwnerSettlementResponseDto(
    Guid Id,
    Guid OwnerId,
    string OwnerName,
    string OwnerPhone,
    string? BankName,
    string? BankAccountNumber,
    string Period,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    int UnitsCount,
    decimal Gross,
    decimal Expense,
    decimal Fee,
    decimal Net,
    SettlementStatus Status,
    string StatusText,
    string StatusBadge,
    string? TransferReference,
    DateTimeOffset? PaidAt,
    string? Note,
    IReadOnlyList<SettlementLineDto> Lines,
    DateTimeOffset CreatedAt
);

public record CreateSettlementRequest(
    Guid OwnerId,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal GrossRentCollected,
    decimal ManagementFeePercent,
    decimal ExpensesDeducted,
    string? Note
);

public record UpdateSettlementStatusRequest(
    SettlementStatus Status,
    string? TransferReference,
    string? Note
);
