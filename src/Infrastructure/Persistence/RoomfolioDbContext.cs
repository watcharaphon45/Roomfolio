using Microsoft.EntityFrameworkCore;
using Roomfolio.Application.Abstractions;
using Roomfolio.Domain.Common;
using Roomfolio.Domain.Deposits;
using Roomfolio.Domain.Finance;
using Roomfolio.Domain.Leasing;
using Roomfolio.Domain.Organizations;
using Roomfolio.Domain.Properties;

namespace Roomfolio.Infrastructure.Persistence;

public class RoomfolioDbContext : DbContext, IRoomfolioDbContext
{
    public RoomfolioDbContext(DbContextOptions<RoomfolioDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<RentCharge> RentCharges => Set<RentCharge>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<PaymentAllocation> PaymentAllocations => Set<PaymentAllocation>();
    public DbSet<OwnerSettlement> OwnerSettlements => Set<OwnerSettlement>();
    public DbSet<OwnerSettlementLine> OwnerSettlementLines => Set<OwnerSettlementLine>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<SecurityDeposit> SecurityDeposits => Set<SecurityDeposit>();
    public DbSet<DepositTransaction> DepositTransactions => Set<DepositTransaction>();
    public DbSet<AgencyFee> AgencyFees => Set<AgencyFee>();
    public DbSet<Organization> Organizations => Set<Organization>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Project
        modelBuilder.Entity<Project>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).IsRequired().HasMaxLength(200);
            b.Property(p => p.Developer).HasMaxLength(200);
            b.Property(p => p.Address).HasMaxLength(500);
            b.Property(p => p.City).HasMaxLength(100);
        });

        // Owner
        modelBuilder.Entity<Owner>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.FullName).IsRequired().HasMaxLength(200);
            b.Property(o => o.Phone).IsRequired().HasMaxLength(50);
            b.Property(o => o.Email).HasMaxLength(100);
            b.Property(o => o.BankName).HasMaxLength(100);
            b.Property(o => o.BankAccountNumber).HasMaxLength(50);
            b.Property(o => o.LineId).HasMaxLength(50);
        });

        // Unit
        modelBuilder.Entity<Unit>(b =>
        {
            b.HasKey(u => u.Id);
            b.Property(u => u.UnitNumber).IsRequired().HasMaxLength(50);
            b.Property(u => u.Building).HasMaxLength(50);
            b.Property(u => u.AreaSqM).HasPrecision(10, 2);
            b.Property(u => u.TargetRentPrice).HasPrecision(12, 2);
            b.Property(u => u.TenantName).HasMaxLength(200);
            b.Property(u => u.Note).HasMaxLength(1000);

            b.HasOne(u => u.Project)
                .WithMany(p => p.Units)
                .HasForeignKey(u => u.ProjectId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(u => u.Owner)
                .WithMany(o => o.Units)
                .HasForeignKey(u => u.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Tenant
        modelBuilder.Entity<Tenant>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.FullName).IsRequired().HasMaxLength(200);
            b.Property(t => t.Phone).IsRequired().HasMaxLength(50);
            b.Property(t => t.Email).HasMaxLength(100);
            b.Property(t => t.IdCardOrPassport).HasMaxLength(50);
            b.Property(t => t.LineId).HasMaxLength(50);
            b.Property(t => t.EmergencyContact).HasMaxLength(200);
            b.Property(t => t.Status).HasMaxLength(20);
            b.Property(t => t.Note).HasMaxLength(1000);
        });

        // Lease
        modelBuilder.Entity<Lease>(b =>
        {
            b.HasKey(l => l.Id);
            b.Property(l => l.LeaseNumber).IsRequired().HasMaxLength(80);
            b.Property(l => l.MonthlyRent).HasPrecision(14, 2);
            b.Property(l => l.DepositAmount).HasPrecision(14, 2);
            b.Property(l => l.TerminationReason).HasMaxLength(500);
            b.Property(l => l.Note).HasMaxLength(1000);

            b.HasOne(l => l.Unit)
                .WithMany(u => u.Leases)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(l => l.Tenant)
                .WithMany(t => t.Leases)
                .HasForeignKey(l => l.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // RentCharge
        modelBuilder.Entity<RentCharge>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.ChargeType).IsRequired().HasMaxLength(50);
            b.Property(r => r.Description).HasMaxLength(250);
            b.Property(r => r.Amount).HasPrecision(14, 2);

            b.HasOne(r => r.Lease)
                .WithMany(l => l.RentCharges)
                .HasForeignKey(r => r.LeaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment
        modelBuilder.Entity<Payment>(b =>
        {
            b.HasKey(p => p.Id);
            b.Property(p => p.Amount).HasPrecision(14, 2);
            b.Property(p => p.ReferenceNo).HasMaxLength(120);
            b.Property(p => p.Note).HasMaxLength(1000);

            b.HasOne(p => p.PayerTenant)
                .WithMany()
                .HasForeignKey(p => p.PayerTenantId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // PaymentAllocation
        modelBuilder.Entity<PaymentAllocation>(b =>
        {
            b.HasKey(a => a.Id);
            b.Property(a => a.AllocatedAmount).HasPrecision(14, 2);
            b.Property(a => a.Note).HasMaxLength(500);

            b.HasOne(a => a.Payment)
                .WithMany(p => p.Allocations)
                .HasForeignKey(a => a.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(a => a.RentCharge)
                .WithMany(r => r.Allocations)
                .HasForeignKey(a => a.RentChargeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OwnerSettlement
        modelBuilder.Entity<OwnerSettlement>(b =>
        {
            b.HasKey(s => s.Id);
            b.Property(s => s.Currency).HasMaxLength(10);
            b.Property(s => s.GrossRentCollected).HasPrecision(14, 2);
            b.Property(s => s.ExpensesDeducted).HasPrecision(14, 2);
            b.Property(s => s.ManagementFeesDeducted).HasPrecision(14, 2);
            b.Property(s => s.CommissionsDeducted).HasPrecision(14, 2);
            b.Property(s => s.OtherAdjustments).HasPrecision(14, 2);
            b.Property(s => s.NetPayable).HasPrecision(14, 2);
            b.Property(s => s.TransferReference).HasMaxLength(120);
            b.Property(s => s.Note).HasMaxLength(1000);

            b.HasOne(s => s.Owner)
                .WithMany(o => o.Settlements)
                .HasForeignKey(s => s.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OwnerSettlementLine
        modelBuilder.Entity<OwnerSettlementLine>(b =>
        {
            b.HasKey(l => l.Id);
            b.Property(l => l.Description).IsRequired().HasMaxLength(250);
            b.Property(l => l.Amount).HasPrecision(14, 2);
            b.Property(l => l.Note).HasMaxLength(500);

            b.HasOne(l => l.OwnerSettlement)
                .WithMany(s => s.Lines)
                .HasForeignKey(l => l.OwnerSettlementId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(l => l.Unit)
                .WithMany()
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Expense
        modelBuilder.Entity<Expense>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Description).IsRequired().HasMaxLength(500);
            b.Property(e => e.Amount).HasPrecision(14, 2);
            b.Property(e => e.ReceiptUrl).HasMaxLength(1000);
            b.Property(e => e.Note).HasMaxLength(1000);

            b.HasOne(e => e.Unit)
                .WithMany()
                .HasForeignKey(e => e.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(e => e.OwnerSettlement)
                .WithMany()
                .HasForeignKey(e => e.OwnerSettlementId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // SecurityDeposit
        modelBuilder.Entity<SecurityDeposit>(b =>
        {
            b.HasKey(d => d.Id);
            b.Property(d => d.InitialAmount).HasPrecision(14, 2);
            b.Property(d => d.DeductedAmount).HasPrecision(14, 2);
            b.Property(d => d.RefundedAmount).HasPrecision(14, 2);
            b.Property(d => d.BankAccountDetails).HasMaxLength(250);
            b.Property(d => d.Note).HasMaxLength(1000);

            b.HasOne(d => d.Lease)
                .WithMany()
                .HasForeignKey(d => d.LeaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // DepositTransaction
        modelBuilder.Entity<DepositTransaction>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.Amount).HasPrecision(14, 2);
            b.Property(t => t.Description).IsRequired().HasMaxLength(500);
            b.Property(t => t.ReferenceNo).HasMaxLength(120);
            b.Property(t => t.Note).HasMaxLength(1000);

            b.HasOne(t => t.SecurityDeposit)
                .WithMany(d => d.Transactions)
                .HasForeignKey(t => t.SecurityDepositId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AgencyFee
        modelBuilder.Entity<AgencyFee>(b =>
        {
            b.HasKey(f => f.Id);
            b.Property(f => f.Description).HasMaxLength(500);
            b.Property(f => f.Period).HasMaxLength(100);
            b.Property(f => f.CalculationBasis).HasMaxLength(250);
            b.Property(f => f.Amount).HasPrecision(14, 2);
            b.Property(f => f.InvoiceNo).HasMaxLength(100);
            b.Property(f => f.Note).HasMaxLength(1000);

            b.HasOne(f => f.Unit)
                .WithMany()
                .HasForeignKey(f => f.UnitId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(f => f.Owner)
                .WithMany()
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(f => f.Lease)
                .WithMany()
                .HasForeignKey(f => f.LeaseId)
                .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(f => f.OwnerSettlement)
                .WithMany()
                .HasForeignKey(f => f.OwnerSettlementId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Organization
        modelBuilder.Entity<Organization>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Name).IsRequired().HasMaxLength(200);
            b.Property(o => o.TaxId).HasMaxLength(50);
            b.Property(o => o.DefaultCurrency).HasMaxLength(20);
            b.Property(o => o.Timezone).HasMaxLength(100);
            b.Property(o => o.PlanName).HasMaxLength(100);
            b.Property(o => o.Address).HasMaxLength(500);
            b.Property(o => o.Phone).HasMaxLength(50);
            b.Property(o => o.Email).HasMaxLength(100);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
