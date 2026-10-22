namespace Roomfolio.Application.Properties;

public record OwnerResponseDto(
    Guid Id,
    string FullName,
    string Phone,
    string? Email,
    string? BankName,
    string? BankAccountNumber,
    string? LineId,
    int UnitsCount,
    IReadOnlyList<string> UnitNumbers,
    DateTimeOffset CreatedAt
);

public record CreateOwnerRequest(
    string FullName,
    string Phone,
    string? Email,
    string? BankName,
    string? BankAccountNumber,
    string? LineId
);

public record UpdateOwnerRequest(
    string FullName,
    string Phone,
    string? Email,
    string? BankName,
    string? BankAccountNumber,
    string? LineId
);
