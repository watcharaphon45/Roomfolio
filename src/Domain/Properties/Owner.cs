using Roomfolio.Domain.Common;
using Roomfolio.Domain.Finance;

namespace Roomfolio.Domain.Properties;

public class Owner : AuditableEntity
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? LineId { get; set; }

    public ICollection<Unit> Units { get; set; } = new List<Unit>();
    public ICollection<OwnerSettlement> Settlements { get; set; } = new List<OwnerSettlement>();
}

