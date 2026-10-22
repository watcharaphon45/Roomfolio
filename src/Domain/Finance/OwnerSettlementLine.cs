using Roomfolio.Domain.Common;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Domain.Finance;

public class OwnerSettlementLine : AuditableEntity
{
    public Guid OwnerSettlementId { get; set; }
    public OwnerSettlement? OwnerSettlement { get; set; }

    public SettlementLineType LineType { get; set; } = SettlementLineType.RentCollected;

    public Guid? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}
