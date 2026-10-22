using Roomfolio.Domain.Common;
using Roomfolio.Domain.Leasing;

namespace Roomfolio.Domain.Deposits;

public class SecurityDeposit : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Guid LeaseId { get; set; }
    public Lease? Lease { get; set; }

    public DepositHolder Holder { get; set; } = DepositHolder.Agency;
    public decimal InitialAmount { get; set; }
    public decimal DeductedAmount { get; set; }
    public decimal RefundedAmount { get; set; }
    public decimal Balance => InitialAmount - DeductedAmount - RefundedAmount;
    
    public DepositStatus Status { get; set; } = DepositStatus.Held;
    public DateOnly? ReceivedDate { get; set; }
    public string? BankAccountDetails { get; set; }
    public string? Note { get; set; }

    public ICollection<DepositTransaction> Transactions { get; set; } = new List<DepositTransaction>();
}
