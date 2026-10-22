using Roomfolio.Domain.Common;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Domain.Finance;

public class Expense : AuditableEntity
{
    public Guid OrganizationId { get; set; }
    
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public ExpenseCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset IncurredDate { get; set; }
    public ExpensePaidBy PaidBy { get; set; }
    public bool IsDeductibleFromOwner { get; set; } = true;
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Confirmed;
    
    public string? ReceiptUrl { get; set; }
    public string? Note { get; set; }

    public Guid? OwnerSettlementId { get; set; }
    public OwnerSettlement? OwnerSettlement { get; set; }
}
