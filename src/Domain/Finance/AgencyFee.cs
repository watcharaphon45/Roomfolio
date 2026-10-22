using Roomfolio.Domain.Common;
using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Domain.Finance;

public class AgencyFee : AuditableEntity
{
    public Guid OrganizationId { get; set; }

    public Guid? LeaseId { get; set; }
    public Lease? Lease { get; set; }

    public Guid? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public AgencyFeeType FeeType { get; set; } = AgencyFeeType.MonthlyManagement;
    public string Description { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string CalculationBasis { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    
    public FeeCollectionMethod CollectionMethod { get; set; } = FeeCollectionMethod.DeductFromSettlement;
    public AgencyFeeStatus Status { get; set; } = AgencyFeeStatus.PendingDeduction;

    public string? InvoiceNo { get; set; }
    public DateTimeOffset? CollectedAt { get; set; }
    public Guid? OwnerSettlementId { get; set; }
    public OwnerSettlement? OwnerSettlement { get; set; }

    public string? Note { get; set; }
}
