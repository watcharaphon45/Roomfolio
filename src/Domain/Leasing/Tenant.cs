using Roomfolio.Domain.Common;

namespace Roomfolio.Domain.Leasing;

public class Tenant : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? IdCardOrPassport { get; set; }
    public string? LineId { get; set; }
    public string? EmergencyContact { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "Active";

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
}
