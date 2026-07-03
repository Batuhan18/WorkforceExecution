using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;

namespace WorkforceExecution.Application.Features.WorkItems.Queries;

// T0: HoM kendisine atanan isleri goruntuler.
public class GetAssignedWorkItemsQuery : IRequest<Result<List<WorkItemDto>>>
{
    public int HomUserId { get; set; }
    public DateTime? Date { get; set; }
}

public class GetAssignedWorkItemsQueryHandler : IRequestHandler<GetAssignedWorkItemsQuery, Result<List<WorkItemDto>>>
{
    private readonly IWorkItemRepository _workItems;

    public GetAssignedWorkItemsQueryHandler(IWorkItemRepository workItems) => _workItems = workItems;

    public async Task<Result<List<WorkItemDto>>> Handle(GetAssignedWorkItemsQuery request, CancellationToken ct)
    {
        var items = await _workItems.GetByHomAsync(request.HomUserId, request.Date, ct);
        return Result<List<WorkItemDto>>.Success(items.Select(WorkItemMapper.ToDto).ToList());
    }
}
