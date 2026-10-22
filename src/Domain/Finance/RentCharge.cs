using Roomfolio.Domain.Common;
using Roomfolio.Domain.Leasing;

namespace Roomfolio.Domain.Finance;

public class RentCharge : AuditableEntity
{
    public Guid LeaseId { get; set; }
    public Lease? Lease { get; set; }

    public string ChargeType { get; set; } = "Rent";
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly DueDate { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public RentChargeStatus Status { get; set; } = RentChargeStatus.Due;

    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
