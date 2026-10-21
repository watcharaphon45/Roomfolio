namespace Roomfolio.Domain.Common;

/// <summary>
/// Marks entities that belong to an organization for multi-tenant isolation.
/// </summary>
public interface ITenantScopedEntity
{
    Guid OrganizationId { get; }
}
