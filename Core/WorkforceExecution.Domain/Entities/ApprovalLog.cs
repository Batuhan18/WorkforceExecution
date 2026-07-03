using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Domain.Entities;

public class ApprovalLog : BaseEntity
{
    public int WorkItemId { get; set; }
    public WorkItem WorkItem { get; set; } = null!;
    public int ApproverId { get; set; }
    public AppUser Approver { get; set; } = null!;
    public UserRole ApproverRole { get; set; }
    public bool IsApproved { get; set; }
    public string? Comment { get; set; }
}
