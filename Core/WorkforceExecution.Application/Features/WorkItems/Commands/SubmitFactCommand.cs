using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Application.Features.WorkItems.Commands;

// Gun Sonu: gerceklesen Quantity, Man-Day, Overtime girilir; kayit onaya gonderilir.
// Is hic baslamadiysa/bitmediyse comment zorunludur (fact <= 0 kurali).
public class SubmitFactCommand : IRequest<Result<WorkItemDto>>
{
    public int WorkItemId { get; set; }
    public int HomUserId { get; set; }
    public decimal FactQuantity { get; set; }
    public decimal FactManday { get; set; }
    public decimal Overtime { get; set; }
    public string? Comment { get; set; }
    public string? ZzzDetail { get; set; }
}

public class SubmitFactCommandHandler : IRequestHandler<SubmitFactCommand, Result<WorkItemDto>>
{
    private readonly IWorkItemRepository _workItems;
    private readonly INotificationService _notifications;

    public SubmitFactCommandHandler(IWorkItemRepository workItems, INotificationService notifications)
    {
        _workItems = workItems;
        _notifications = notifications;
    }

    public async Task<Result<WorkItemDto>> Handle(SubmitFactCommand request, CancellationToken ct)
    {
        var item = await _workItems.GetWithDetailsAsync(request.WorkItemId, ct);
        if (item is null) return Result<WorkItemDto>.Failure("Is kaydi bulunamadi.");
        if (item.AssignedHomId != request.HomUserId)
            return Result<WorkItemDto>.Failure("Bu is size atanmamis.");
        if (item.Status != WorkItemStatus.InProgress && item.Status != WorkItemStatus.Rejected)
            return Result<WorkItemDto>.Failure("Fact girisi icin is 'InProgress' veya 'Rejected' durumunda olmalidir.");

        // Is hic baslamadi ya da bitmediyse aciklama zorunlu.
        if (request.FactQuantity <= 0 && string.IsNullOrWhiteSpace(request.Comment))
            return Result<WorkItemDto>.Failure("Is baslamadiysa veya bitmediyse 'comment' girilmesi zorunludur.");

        item.FactQuantity = request.FactQuantity;
        item.FactManday = request.FactManday;
        item.Overtime = request.Overtime;
        item.Comment = request.Comment;
        item.ZzzDetail = request.ZzzDetail;
        item.Status = WorkItemStatus.PendingHomApproval;

        _workItems.Update(item);
        await _workItems.SaveChangesAsync(ct);

        var dto = WorkItemMapper.ToDto(item);
        await _notifications.NotifyUserAsync(dto.AssignedHomEmail, "FactSubmitted", dto, ct);
        return Result<WorkItemDto>.Success(dto);
    }
}
