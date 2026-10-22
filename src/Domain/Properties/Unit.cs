using Roomfolio.Domain.Common;
using Roomfolio.Domain.Leasing;

namespace Roomfolio.Domain.Properties;

public class Unit : AuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }

    public Guid? OwnerId { get; set; }
    public Owner? Owner { get; set; }

    public string UnitNumber { get; set; } = string.Empty;
    public int Floor { get; set; }
    public string? Building { get; set; }
    public int Bedrooms { get; set; } = 1;
    public int Bathrooms { get; set; } = 1;
    public decimal AreaSqM { get; set; }
    public decimal TargetRentPrice { get; set; }
    public UnitStatus Status { get; set; } = UnitStatus.Available;
    public RentalPaymentMode PaymentMode { get; set; } = RentalPaymentMode.DirectToOwner;
    public string? TenantName { get; set; }
    public string? Note { get; set; }

    public ICollection<Lease> Leases { get; set; } = new List<Lease>();
}

