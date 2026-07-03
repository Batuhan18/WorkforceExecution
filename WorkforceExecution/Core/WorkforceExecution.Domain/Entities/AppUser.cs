using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Domain.Entities;

public class AppUser : BaseEntity
{
    public string Email { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }

    // HeadOfMaster icin lokasyon, TechOffice/SiteChief icin bolge atamasi.
    public int? LocationId { get; set; }
    public Location? Location { get; set; }
    public int? CrewRegionId { get; set; }
    public CrewRegion? CrewRegion { get; set; }
}
