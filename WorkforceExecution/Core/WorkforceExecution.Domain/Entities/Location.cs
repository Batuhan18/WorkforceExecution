namespace WorkforceExecution.Domain.Entities;

// Lokasyon (KKK): her lokasyona bir Head of Master atanir.
public class Location : BaseEntity
{
    public string Code { get; set; } = null!; // orn. 30AAA
    public int CrewRegionId { get; set; }
    public CrewRegion CrewRegion { get; set; } = null!;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}
