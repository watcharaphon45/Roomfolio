using Roomfolio.Domain.Common;
using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Domain.Leasing;

public class Lease : AuditableEntity
{
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public string LeaseNumber { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int RentDueDay { get; set; } = 1;
    public decimal MonthlyRent { get; set; }
    public RentalPaymentMode PaymentMode { get; set; } = RentalPaymentMode.DirectToOwner;
    public decimal DepositAmount { get; set; }
    public DepositHolder DepositHolder { get; set; } = DepositHolder.Owner;
    public LeaseStatus Status { get; set; } = LeaseStatus.Active;

    public DateOnly? SignedAt { get; set; }
    public DateOnly? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }
    public string? Note { get; set; }

    public ICollection<RentCharge> RentCharges { get; set; } = new List<RentCharge>();
}

