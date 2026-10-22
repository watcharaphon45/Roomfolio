namespace Roomfolio.Domain.Finance;

public enum RentChargeStatus
{
    Draft = 1,
    Due = 2,
    PartiallyPaid = 3,
    Paid = 4,
    Overdue = 5,
    Cancelled = 6
}

public enum PaymentParty
{
    Tenant = 1,
    Owner = 2,
    Agency = 3,
    Other = 4
}

public enum PaymentMethod
{
    BankTransfer = 1,
    Cash = 2,
    PromptPayQR = 3,
    Other = 4
}

public enum PaymentStatus
{
    Recorded = 1,
    Confirmed = 2,
    Voided = 3
}

public enum SettlementStatus
{
    Draft = 1,
    Review = 2,
    Approved = 3,
    Paid = 4,
    Cancelled = 5
}

public enum SettlementLineType
{
    RentCollected = 1,
    ManagementFee = 2,
    Commission = 3,
    Expense = 4,
    Adjustment = 5
}

public enum ExpenseCategory
{
    Cleaning = 1,
    Repair = 2,
    Keycard = 3,
    Aircon = 4,
    CommonFee = 5,
    Other = 6
}

public enum ExpensePaidBy
{
    Agency = 1,
    Owner = 2,
    Tenant = 3
}

public enum ExpenseStatus
{
    Pending = 1,
    Confirmed = 2,
    Settled = 3,
    Voided = 4
}

public enum AgencyFeeType
{
    MonthlyManagement = 1,
    LeasingCommission = 2,
    RenewalFee = 3,
    OtherService = 4
}

public enum FeeCollectionMethod
{
    DeductFromSettlement = 1,
    SeparateInvoice = 2,
    DirectPayment = 3
}

public enum AgencyFeeStatus
{
    PendingDeduction = 1,
    Invoiced = 2,
    Collected = 3,
    Voided = 4
}

