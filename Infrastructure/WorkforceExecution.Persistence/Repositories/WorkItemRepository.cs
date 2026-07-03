using Microsoft.EntityFrameworkCore;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Domain.Enums;
using WorkforceExecution.Persistence.Context;

namespace WorkforceExecution.Persistence.Repositories;

public class WorkItemRepository : Repository<WorkItem>, IWorkItemRepository
{
    public WorkItemRepository(AppDbContext context) : base(context) { }

    // Detay ekranlari icin tum navigation'lar dahil tek sorgu.
    private IQueryable<WorkItem> WithDetails() => Context.WorkItems
        .Include(w => w.Location).ThenInclude(l => l.CrewRegion)
        .Include(w => w.Project)
        .Include(w => w.SubSubTypeOfWork).ThenInclude(s => s.SubTypeOfWork).ThenInclude(s => s.TypeOfWork)
        .Include(w => w.SubmittedBy)
        .Include(w => w.AssignedHom)
        .Include(w => w.CrewAssignments).ThenInclude(c => c.WorkerType)
        .Include(w => w.ApprovalLogs).ThenInclude(a => a.Approver);

    public Task<WorkItem?> GetWithDetailsAsync(int id, CancellationToken ct = default)
        => WithDetails().FirstOrDefaultAsync(w => w.Id == id, ct);

    public Task<List<WorkItem>> GetByHomAsync(int homUserId, DateTime? date, CancellationToken ct = default)
    {
        var query = WithDetails().Where(w => w.AssignedHomId == homUserId);
        if (date.HasValue) query = query.Where(w => w.ReportDate == date.Value.Date);
        return query.OrderByDescending(w => w.Id).ToListAsync(ct);
    }

    public Task<List<WorkItem>> GetBySubmitterAsync(int techOfficeUserId, DateTime? date, CancellationToken ct = default)
    {
        var query = WithDetails().Where(w => w.SubmittedById == techOfficeUserId);
        if (date.HasValue) query = query.Where(w => w.ReportDate == date.Value.Date);
        return query.OrderByDescending(w => w.Id).ToListAsync(ct);
    }

    public Task<List<WorkItem>> GetByStatusForRegionAsync(WorkItemStatus status, int? crewRegionId, DateTime? date, CancellationToken ct = default)
    {
        var query = WithDetails().Where(w => w.Status == status);
        if (crewRegionId.HasValue) query = query.Where(w => w.Location.CrewRegionId == crewRegionId.Value);
        if (date.HasValue) query = query.Where(w => w.ReportDate == date.Value.Date);
        return query.OrderByDescending(w => w.Id).ToListAsync(ct);
    }

    public Task<List<WorkItem>> GetApprovedAsync(DateTime date, CancellationToken ct = default)
        => WithDetails()
            .Where(w => w.Status == WorkItemStatus.Approved && w.ReportDate == date.Date)
            .OrderBy(w => w.Location.Code)
            .ToListAsync(ct);
}
