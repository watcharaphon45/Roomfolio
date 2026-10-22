using Roomfolio.Domain.Common;

namespace Roomfolio.Domain.Finance;

public class PaymentAllocation : AuditableEntity
{
    public Guid PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public Guid RentChargeId { get; set; }
    public RentCharge? RentCharge { get; set; }

    public decimal AllocatedAmount { get; set; }
    public string? Note { get; set; }
}
