namespace Roomfolio.Domain.Leasing;

public enum LeaseStatus
{
    Draft = 1,
    Active = 2,
    ExpiringSoon = 3,
    Ended = 4,
    Terminated = 5
}

public enum DepositHolder
{
    Owner = 1,
    Agency = 2,
    Other = 3
}
