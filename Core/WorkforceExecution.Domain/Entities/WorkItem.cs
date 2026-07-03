using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Domain.Entities;

// Gunluk planin tek satiri: planlanan is + gerceklesen degerler + onay durumu.
public class WorkItem : BaseEntity
{
    public DateTime ReportDate { get; set; }

    public int LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int SubSubTypeOfWorkId { get; set; }
    public SubSubTypeOfWork SubSubTypeOfWork { get; set; } = null!;

    // Plan (Teknik Ofis girer)
    public decimal PlannedQuantity { get; set; }
    public decimal PlannedManday { get; set; }

    // Fact (Head of Master gun sonu girer)
    public decimal? FactQuantity { get; set; }
    public decimal? FactManday { get; set; }
    public decimal? Overtime { get; set; }
    public string? Comment { get; set; }   // is baslamadi / bitmedi aciklamasi
    public string? ZzzDetail { get; set; } // ZZZ detay bilgisi

    public WorkItemStatus Status { get; set; } = WorkItemStatus.Planned;

    public int SubmittedById { get; set; }      // Teknik Ofis kullanicisi
    public AppUser SubmittedBy { get; set; } = null!;
    public int AssignedHomId { get; set; }      // Atanan Head of Master
    public AppUser AssignedHom { get; set; } = null!;

    public ICollection<CrewAssignment> CrewAssignments { get; set; } = new List<CrewAssignment>();
    public ICollection<ApprovalLog> ApprovalLogs { get; set; } = new List<ApprovalLog>();
}
