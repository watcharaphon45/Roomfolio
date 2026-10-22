using Roomfolio.Domain.Common;

namespace Roomfolio.Domain.Organizations;

public class Organization : AuditableEntity
{
    public string Name { get; set; } = "Prime Living Agency Co., Ltd.";
    public string TaxId { get; set; } = "0105565012345";
    public string DefaultCurrency { get; set; } = "THB (฿)";
    public string Timezone { get; set; } = "Asia/Bangkok (UTC+07:00)";
    public string PlanName { get; set; } = "Pro Plan";
    public decimal PlanPrice { get; set; } = 799;
    public int RoomLimit { get; set; } = 200;
    public string? Address { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
