namespace WorkforceExecution.Domain.Entities;

public class Project : BaseEntity
{
    public string Code { get; set; } = null!; // orn. AAA.0199.30AAA.0.KK.TZ0003
    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;
}
