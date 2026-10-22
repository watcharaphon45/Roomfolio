using Roomfolio.Domain.Common;

namespace Roomfolio.Domain.Properties;

public class Project : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Developer { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public int? TotalFloors { get; set; }

    public ICollection<Unit> Units { get; set; } = new List<Unit>();
}
