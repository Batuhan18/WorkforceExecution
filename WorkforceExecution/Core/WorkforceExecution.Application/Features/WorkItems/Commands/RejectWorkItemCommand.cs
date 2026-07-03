using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Application.Features.WorkItems.Commands;

// Red: kayit 'Rejected' durumuna doner, HoM duzeltip yeniden fact girebilir.
public class RejectWorkItemCommand : IRequest<Result<WorkItemDto>>
{
    public int WorkItemId { get; set; }
    public int ApproverId { get; set; }
    public string Comment { get; set; } = null!;
}

public class RejectWorkItemCommandHandler : IRequestHandler<RejectWorkItemCommand, Result<WorkItemDto>>
{
    private readonly IWorkItemRepository _workItems;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;

    public RejectWorkItemCommandHandler(IWorkItemRepository workItems, IUserRepository users, INotificationService notifications)
    {
        _workItems = workItems;
        _users = users;
        _notifications = notifications;
    }

    public async Task<Result<WorkItemDto>> Handle(RejectWorkItemCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Comment))
            return Result<WorkItemDto>.Failure("Red icin aciklama (comment) zorunludur.");

        var item = await _workItems.GetWithDetailsAsync(request.WorkItemId, ct);
        if (item is null) return Result<WorkItemDto>.Failure("Is kaydi bulunamadi.");

        var approver = await _users.GetByIdAsync(request.ApproverId, ct);
        if (approver is null) return Result<WorkItemDto>.Failure("Kullanici bulunamadi.");

        var canReject = item.Status switch
        {
            WorkItemStatus.PendingHomApproval => approver.Role == UserRole.HeadOfMaster && item.AssignedHomId == approver.Id,
            WorkItemStatus.PendingSiteChief   => approver.Role == UserRole.SiteChief && approver.CrewRegionId == item.Location.CrewRegionId,
            WorkItemStatus.PendingPm          => approver.Role == UserRole.ProjectManager,
            _ => false
        };
        if (!canReject)
            return Result<WorkItemDto>.Failure("Bu kaydi bu asamada reddetme yetkiniz yok.");

        item.Status = WorkItemStatus.Rejected;
        item.ApprovalLogs.Add(new ApprovalLog
        {
            WorkItemId = item.Id,
            ApproverId = approver.Id,
            ApproverRole = approver.Role,
            IsApproved = false,
            Comment = request.Comment
        });

        _workItems.Update(item);
        await _workItems.SaveChangesAsync(ct);

        var dto = WorkItemMapper.ToDto(item);
        await _notifications.NotifyUserAsync(dto.AssignedHomEmail, "WorkItemRejected", dto, ct);
        await _notifications.NotifyUserAsync(dto.SubmittedByEmail, "WorkItemRejected", dto, ct);
        return Result<WorkItemDto>.Success(dto);
    }
}
