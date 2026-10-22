using Roomfolio.Domain.Common;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Domain.Finance;

public class OwnerSettlement : AuditableEntity
{
    public Guid OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }

    public SettlementStatus Status { get; set; } = SettlementStatus.Review;
    public string Currency { get; set; } = "THB";

    public decimal GrossRentCollected { get; set; }
    public decimal ExpensesDeducted { get; set; }
    public decimal ManagementFeesDeducted { get; set; }
    public decimal CommissionsDeducted { get; set; }
    public decimal OtherAdjustments { get; set; }
    public decimal NetPayable { get; set; }

    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string? TransferReference { get; set; }
    public string? Note { get; set; }

    public ICollection<OwnerSettlementLine> Lines { get; set; } = new List<OwnerSettlementLine>();
}
