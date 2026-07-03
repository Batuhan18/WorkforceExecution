using WorkforceExecution.Domain.Entities;

namespace WorkforceExecution.Application.Dtos;

// Junior+ pragmatizmi: AutoMapper yerine okunabilir, elle yazilmis tek mapper.
public static class WorkItemMapper
{
    public static WorkItemDto ToDto(WorkItem w) => new()
    {
        Id = w.Id,
        ReportDate = w.ReportDate,
        LocationCode = w.Location.Code,
        ProjectCode = w.Project.Code,
        TowCode = w.SubSubTypeOfWork.SubTypeOfWork.TypeOfWork.Code,
        StowCode = w.SubSubTypeOfWork.SubTypeOfWork.Code,
        SstowCode = w.SubSubTypeOfWork.Code,
        Unit = w.SubSubTypeOfWork.Unit,
        TypeOfCode = w.SubSubTypeOfWork.TypeOfCode,
        PlannedQuantity = w.PlannedQuantity,
        PlannedManday = w.PlannedManday,
        FactQuantity = w.FactQuantity,
        FactManday = w.FactManday,
        Overtime = w.Overtime,
        Comment = w.Comment,
        ZzzDetail = w.ZzzDetail,
        Status = w.Status.ToString(),
        SubmittedByEmail = w.SubmittedBy.Email,
        AssignedHomEmail = w.AssignedHom.Email,
        Efficiency = (w.FactQuantity.HasValue && w.PlannedQuantity > 0 && w.FactQuantity.Value >= 0)
            ? Math.Round(w.FactQuantity.Value / w.PlannedQuantity * 100, 1)
            : null,
        Crews = w.CrewAssignments.Select(c => new CrewAssignmentDto
        {
            WorkerTypeId = c.WorkerTypeId,
            WorkerTypeName = c.WorkerType.Name,
            WorkerCount = c.WorkerCount
        }).ToList(),
        Approvals = w.ApprovalLogs.OrderBy(a => a.CreatedAt).Select(a => new ApprovalLogDto
        {
            ApproverEmail = a.Approver.Email,
            ApproverRole = a.ApproverRole.ToString(),
            IsApproved = a.IsApproved,
            Comment = a.Comment,
            CreatedAt = a.CreatedAt
        }).ToList()
    };
}
