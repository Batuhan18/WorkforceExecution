using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Application.Features.WorkItems.Queries;

// Rolun onay kuyrugu: HoM kendi lokasyonunu, SC kendi bolgesini, PM tum projeyi gorur.
public class GetPendingApprovalsQuery : IRequest<Result<List<WorkItemDto>>>
{
    public int UserId { get; set; }
    public DateTime? Date { get; set; }
}

public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, Result<List<WorkItemDto>>>
{
    private readonly IWorkItemRepository _workItems;
    private readonly IUserRepository _users;

    public GetPendingApprovalsQueryHandler(IWorkItemRepository workItems, IUserRepository users)
    {
        _workItems = workItems;
        _users = users;
    }

    public async Task<Result<List<WorkItemDto>>> Handle(GetPendingApprovalsQuery request, CancellationToken ct)
    {
        var user = await _users.GetByIdAsync(request.UserId, ct);
        if (user is null) return Result<List<WorkItemDto>>.Failure("Kullanici bulunamadi.");

        var items = user.Role switch
        {
            UserRole.HeadOfMaster => (await _workItems.GetByHomAsync(user.Id, request.Date, ct))
                .Where(w => w.Status == WorkItemStatus.PendingHomApproval).ToList(),
            UserRole.SiteChief => await _workItems.GetByStatusForRegionAsync(WorkItemStatus.PendingSiteChief, user.CrewRegionId, request.Date, ct),
            UserRole.ProjectManager => await _workItems.GetByStatusForRegionAsync(WorkItemStatus.PendingPm, null, request.Date, ct),
            _ => new List<Domain.Entities.WorkItem>()
        };

        return Result<List<WorkItemDto>>.Success(items.Select(WorkItemMapper.ToDto).ToList());
    }
}
