using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Application.Interfaces;

public interface IWorkItemRepository : IRepository<WorkItem>
{
    Task<WorkItem?> GetWithDetailsAsync(int id, CancellationToken ct = default);
    Task<List<WorkItem>> GetByHomAsync(int homUserId, DateTime? date, CancellationToken ct = default);
    Task<List<WorkItem>> GetBySubmitterAsync(int techOfficeUserId, DateTime? date, CancellationToken ct = default);
    Task<List<WorkItem>> GetByStatusForRegionAsync(WorkItemStatus status, int? crewRegionId, DateTime? date, CancellationToken ct = default);
    Task<List<WorkItem>> GetApprovedAsync(DateTime date, CancellationToken ct = default);
}
