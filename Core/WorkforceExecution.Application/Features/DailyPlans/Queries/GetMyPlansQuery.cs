using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;

namespace WorkforceExecution.Application.Features.DailyPlans.Queries;

public class GetMyPlansQuery : IRequest<Result<List<WorkItemDto>>>
{
    public int TechOfficeUserId { get; set; }
    public DateTime? Date { get; set; }
}

public class GetMyPlansQueryHandler : IRequestHandler<GetMyPlansQuery, Result<List<WorkItemDto>>>
{
    private readonly IWorkItemRepository _workItems;

    public GetMyPlansQueryHandler(IWorkItemRepository workItems) => _workItems = workItems;

    public async Task<Result<List<WorkItemDto>>> Handle(GetMyPlansQuery request, CancellationToken ct)
    {
        var items = await _workItems.GetBySubmitterAsync(request.TechOfficeUserId, request.Date, ct);
        return Result<List<WorkItemDto>>.Success(items.Select(WorkItemMapper.ToDto).ToList());
    }
}
