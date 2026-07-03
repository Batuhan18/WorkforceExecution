namespace WorkforceExecution.Domain.Entities;

// HoM'un ise atadigi ekip: worker type + kisi sayisi.
public class CrewAssignment : BaseEntity
{
    public int WorkItemId { get; set; }
    public WorkItem WorkItem { get; set; } = null!;
    public int WorkerTypeId { get; set; }
    public WorkerType WorkerType { get; set; } = null!;
    public int WorkerCount { get; set; }
}
