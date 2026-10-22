using Roomfolio.Domain.Common;

namespace Roomfolio.Domain.Deposits;

public class DepositTransaction : AuditableEntity
{
    public Guid SecurityDepositId { get; set; }
    public SecurityDeposit? SecurityDeposit { get; set; }

    public DepositTransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public string? Note { get; set; }
}
