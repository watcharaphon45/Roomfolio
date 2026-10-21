namespace Roomfolio.Application.Abstractions;

public interface ITenantProvider
{
    Guid? CurrentOrganizationId { get; }
    void SetCurrentOrganizationId(Guid organizationId);
}
