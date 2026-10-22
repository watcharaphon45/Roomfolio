using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Application.Finance;

public record RecordPaymentRequest(
    Guid RentChargeId,
    decimal Amount,
    DateOnly PaymentDate,
    PaymentMethod Method,
    RentalPaymentMode PaymentMode,
    PaymentParty ReceivedByParty,
    string? ReferenceNo,
    string? Note
);

public record PaymentResponseDto(
    Guid Id,
    Guid RentChargeId,
    decimal Amount,
    DateOnly PaymentDate,
    PaymentMethod Method,
    string MethodText,
    RentalPaymentMode PaymentMode,
    string PaymentModeText,
    PaymentParty ReceivedByParty,
    string ReceivedByPartyText,
    string? ReferenceNo,
    PaymentStatus Status,
    string? Note,
    DateTimeOffset CreatedAt
);
