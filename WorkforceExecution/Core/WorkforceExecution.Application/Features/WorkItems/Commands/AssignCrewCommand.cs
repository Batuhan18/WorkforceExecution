using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Application.Features.WorkItems.Commands;

// T0: HoM crew olusturur (worker type + kisi sayisi) ve isi alir.
public class AssignCrewCommand : IRequest<Result<WorkItemDto>>
{
    public int WorkItemId { get; set; }
    public int HomUserId { get; set; }
    public List<CrewItemDto> Crews { get; set; } = new();
}

public class CrewItemDto
{
    public int WorkerTypeId { get; set; }
    public int WorkerCount { get; set; }
}

public class AssignCrewCommandHandler : IRequestHandler<AssignCrewCommand, Result<WorkItemDto>>
{
    private readonly IWorkItemRepository _workItems;
    private readonly INotificationService _notifications;

    public AssignCrewCommandHandler(IWorkItemRepository workItems, INotificationService notifications)
    {
        _workItems = workItems;
        _notifications = notifications;
    }

    public async Task<Result<WorkItemDto>> Handle(AssignCrewCommand request, CancellationToken ct)
    {
        var item = await _workItems.GetWithDetailsAsync(request.WorkItemId, ct);
        if (item is null) return Result<WorkItemDto>.Failure("Is kaydi bulunamadi.");
        if (item.AssignedHomId != request.HomUserId)
            return Result<WorkItemDto>.Failure("Bu is size atanmamis.");
        if (item.Status != WorkItemStatus.Planned)
            return Result<WorkItemDto>.Failure("Crew sadece 'Planned' durumundaki ise atanabilir.");
        if (request.Crews.Count == 0 || request.Crews.Any(c => c.WorkerCount <= 0))
            return Result<WorkItemDto>.Failure("Gecerli en az bir crew satiri girilmelidir.");

        foreach (var c in request.Crews)
        {
            item.CrewAssignments.Add(new CrewAssignment
            {
                WorkItemId = item.Id,
                WorkerTypeId = c.WorkerTypeId,
                WorkerCount = c.WorkerCount
            });
        }

        item.Status = WorkItemStatus.InProgress;
        _workItems.Update(item);
        await _workItems.SaveChangesAsync(ct);

        var full = await _workItems.GetWithDetailsAsync(item.Id, ct);
        var dto = WorkItemMapper.ToDto(full!);
        await _notifications.NotifyUserAsync(dto.SubmittedByEmail, "CrewAssigned", dto, ct);
        return Result<WorkItemDto>.Success(dto);
    }
}
