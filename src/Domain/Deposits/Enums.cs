namespace Roomfolio.Domain.Deposits;

public enum DepositStatus
{
    Held = 1,              // ถือครองอยู่
    PartiallyDeducted = 2, // หักชดเชยบางส่วน
    FullyRefunded = 3,     // คืนเงินแล้วเสร็จ
    Forfeited = 4          // ริบเงินประกัน
}

public enum DepositTransactionType
{
    InitialDeposit = 1,    // รับเงินประกันเริ่มต้น
    Deduction = 2,         // หักชดเชยความเสียหาย/ค่าทำความสะอาด
    Refund = 3,            // คืนเงินประกันให้ผู้เช่า
    Forfeit = 4            // ริบเงินประกันเข้าเจ้าของ/นายหน้า
}
