using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Application.Features.WorkItems.Commands;

// Onay zinciri: Head of Master -> Site Chief -> Project Manager.
public class ApproveWorkItemCommand : IRequest<Result<WorkItemDto>>
{
    public int WorkItemId { get; set; }
    public int ApproverId { get; set; }
    public string? Comment { get; set; }
}

public class ApproveWorkItemCommandHandler : IRequestHandler<ApproveWorkItemCommand, Result<WorkItemDto>>
{
    private readonly IWorkItemRepository _workItems;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;

    public ApproveWorkItemCommandHandler(IWorkItemRepository workItems, IUserRepository users, INotificationService notifications)
    {
        _workItems = workItems;
        _users = users;
        _notifications = notifications;
    }   

    public async Task<Result<WorkItemDto>> Handle(ApproveWorkItemCommand request, CancellationToken ct)
    {
        var item = await _workItems.GetWithDetailsAsync(request.WorkItemId, ct);
        if (item is null) return Result<WorkItemDto>.Failure("Is kaydi bulunamadi.");

        var approver = await _users.GetByIdAsync(request.ApproverId, ct);
        if (approver is null) return Result<WorkItemDto>.Failure("Onaylayan kullanici bulunamadi.");

        // Durum + rol + yetki alani kontrolu (dogru kisi dogru asamada onayliyor mu?)
        switch (item.Status)
        {
            case WorkItemStatus.PendingHomApproval:
                if (approver.Role != UserRole.HeadOfMaster || item.AssignedHomId != approver.Id)
                    return Result<WorkItemDto>.Failure("Bu asamada onay yetkisi atanan Head of Master'dadir.");
                item.Status = WorkItemStatus.PendingSiteChief;
                break;

            case WorkItemStatus.PendingSiteChief:
                if (approver.Role != UserRole.SiteChief || approver.CrewRegionId != item.Location.CrewRegionId)
                    return Result<WorkItemDto>.Failure("Bu asamada onay yetkisi ilgili bolgenin Site Chief'indedir.");
                item.Status = WorkItemStatus.PendingPm;
                break;

            case WorkItemStatus.PendingPm:
                if (approver.Role != UserRole.ProjectManager)
                    return Result<WorkItemDto>.Failure("Bu asamada onay yetkisi Project Manager'dadir.");
                item.Status = WorkItemStatus.Approved;
                break;

            default:
                return Result<WorkItemDto>.Failure($"'{item.Status}' durumundaki kayit onaylanamaz.");
        }

        item.ApprovalLogs.Add(new ApprovalLog
        {
            WorkItemId = item.Id,
            ApproverId = approver.Id,
            ApproverRole = approver.Role,
            IsApproved = true,
            Comment = request.Comment
        });

        _workItems.Update(item);
        await _workItems.SaveChangesAsync(ct);

        var dto = WorkItemMapper.ToDto(item);

        // Zincirdeki siradaki role ve plani olusturana canli bildirim.
        switch (item.Status)
        {
            case WorkItemStatus.PendingSiteChief:
                await _notifications.NotifyRoleAsync("SiteChief", "ApprovalRequested", dto, ct);
                break;
            case WorkItemStatus.PendingPm:
                await _notifications.NotifyRoleAsync("ProjectManager", "ApprovalRequested", dto, ct);
                break;
            case WorkItemStatus.Approved:
                await _notifications.NotifyUserAsync(dto.SubmittedByEmail, "WorkItemApproved", dto, ct);
                await _notifications.NotifyUserAsync(dto.AssignedHomEmail, "WorkItemApproved", dto, ct);
                // Onaylanan kayit Daily Report'ta konsolide edilir -> rapor ekranlari canli yenilensin.
                await _notifications.BroadcastAsync("DailyReportUpdated", new { dto.ReportDate }, ct);
                break;
        }

        return Result<WorkItemDto>.Success(dto);
    }
}
