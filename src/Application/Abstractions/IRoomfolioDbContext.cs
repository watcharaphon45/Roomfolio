using Microsoft.EntityFrameworkCore;
using Roomfolio.Domain.Deposits;
using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Organizations;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Application.Abstractions;

public interface IRoomfolioDbContext
{
    DbSet<Project> Projects { get; }
    DbSet<Owner> Owners { get; }
    DbSet<Unit> Units { get; }
    DbSet<Tenant> Tenants { get; }
    DbSet<Lease> Leases { get; }
    DbSet<RentCharge> RentCharges { get; }
    DbSet<Payment> Payments { get; }
    DbSet<PaymentAllocation> PaymentAllocations { get; }
    DbSet<OwnerSettlement> OwnerSettlements { get; }
    DbSet<OwnerSettlementLine> OwnerSettlementLines { get; }
    DbSet<Expense> Expenses { get; }
    DbSet<SecurityDeposit> SecurityDeposits { get; }
    DbSet<DepositTransaction> DepositTransactions { get; }
    DbSet<AgencyFee> AgencyFees { get; }
    DbSet<Organization> Organizations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}


