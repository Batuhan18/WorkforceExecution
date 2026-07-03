namespace WorkforceExecution.Application.Dtos;

public class CrewAssignmentDto
{
    public int WorkerTypeId { get; set; }
    public string WorkerTypeName { get; set; } = null!;
    public int WorkerCount { get; set; }
}

public class ApprovalLogDto
{
    public string ApproverEmail { get; set; } = null!;
    public string ApproverRole { get; set; } = null!;
    public bool IsApproved { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WorkItemDto
{
    public int Id { get; set; }
    public DateTime ReportDate { get; set; }
    public string LocationCode { get; set; } = null!;
    public string ProjectCode { get; set; } = null!;
    public string TowCode { get; set; } = null!;
    public string StowCode { get; set; } = null!;
    public string SstowCode { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public string TypeOfCode { get; set; } = null!;
    public decimal PlannedQuantity { get; set; }
    public decimal PlannedManday { get; set; }
    public decimal? FactQuantity { get; set; }
    public decimal? FactManday { get; set; }
    public decimal? Overtime { get; set; }
    public string? Comment { get; set; }
    public string? ZzzDetail { get; set; }
    public string Status { get; set; } = null!;
    public string SubmittedByEmail { get; set; } = null!;
    public string AssignedHomEmail { get; set; } = null!;
    public decimal? Efficiency { get; set; } // FactQty / PlannedQty
    public List<CrewAssignmentDto> Crews { get; set; } = new();
    public List<ApprovalLogDto> Approvals { get; set; } = new();
}
