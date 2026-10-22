using Roomfolio.Domain.Common;
using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Domain.Finance;

public class Payment : AuditableEntity
{
    public PaymentParty PayerPartyType { get; set; } = PaymentParty.Tenant;
    public Guid? PayerTenantId { get; set; }
    public Tenant? PayerTenant { get; set; }

    public PaymentParty ReceivedByParty { get; set; } = PaymentParty.Agency;
    public RentalPaymentMode PaymentMode { get; set; } = RentalPaymentMode.ThroughAgent;

    public DateOnly PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; } = PaymentMethod.BankTransfer;
    public string? ReferenceNo { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Confirmed;
    public string? Note { get; set; }

    public ICollection<PaymentAllocation> Allocations { get; set; } = new List<PaymentAllocation>();
}
