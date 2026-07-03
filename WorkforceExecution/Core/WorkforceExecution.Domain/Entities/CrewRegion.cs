namespace WorkforceExecution.Domain.Entities;

// Bolge: her bolgenin bir tech office ve bir site chief kullanicisi vardir.
public class CrewRegion : BaseEntity
{
    public string Name { get; set; } = null!; // orn. abolge, bbolge
    public ICollection<Location> Locations { get; set; } = new List<Location>();
}
