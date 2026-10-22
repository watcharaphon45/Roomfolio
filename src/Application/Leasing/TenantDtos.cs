namespace Roomfolio.Application.Leasing;

public record TenantResponseDto(
    Guid Id,
    string FullName,
    string Phone,
    string? Email,
    string? IdCardOrPassport,
    string? LineId,
    string? EmergencyContact,
    string Status,
    string? Note,
    int ActiveLeasesCount,
    string? CurrentUnit,
    string? CurrentLeaseNumber,
    decimal? CurrentDeposit,
    DateTimeOffset CreatedAt
);

public record CreateTenantRequest(
    string FullName,
    string Phone,
    string? Email,
    string? IdCardOrPassport,
    string? LineId,
    string? EmergencyContact,
    string? Note
);

public record UpdateTenantRequest(
    string FullName,
    string Phone,
    string? Email,
    string? IdCardOrPassport,
    string? LineId,
    string? EmergencyContact,
    string Status,
    string? Note
);

public record TenantLookupDto(Guid Id, string FullName, string Phone);
