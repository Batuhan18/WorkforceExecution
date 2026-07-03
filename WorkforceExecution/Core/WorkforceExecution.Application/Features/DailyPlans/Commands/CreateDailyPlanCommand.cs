using MediatR;
using WorkforceExecution.Application.Common;
using WorkforceExecution.Application.Dtos;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.Domain.Entities;
using WorkforceExecution.Domain.Enums;

namespace WorkforceExecution.Application.Features.DailyPlans.Commands;

// T-1: Teknik Ofis gunluk plani olusturur; her satir ilgili lokasyonun HoM'una atanir.
public class CreateDailyPlanCommand : IRequest<Result<List<WorkItemDto>>>
{
    public int SubmittedById { get; set; } // Controller, JWT claim'inden doldurur
    public DateTime ReportDate { get; set; }
    public List<DailyPlanItemDto> Items { get; set; } = new();
}

public class DailyPlanItemDto
{
    public int LocationId { get; set; }
    public int ProjectId { get; set; }
    public int SubSubTypeOfWorkId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public decimal PlannedManday { get; set; }
}

public class CreateDailyPlanCommandHandler : IRequestHandler<CreateDailyPlanCommand, Result<List<WorkItemDto>>>
{
    private readonly IWorkItemRepository _workItems;
    private readonly IUserRepository _users;
    private readonly INotificationService _notifications;

    public CreateDailyPlanCommandHandler(IWorkItemRepository workItems, IUserRepository users, INotificationService notifications)
    {
        _workItems = workItems;
        _users = users;
        _notifications = notifications;
    }

    public async Task<Result<List<WorkItemDto>>> Handle(CreateDailyPlanCommand request, CancellationToken ct)
    {
        if (request.Items.Count == 0)
            return Result<List<WorkItemDto>>.Failure("Plan en az bir satir icermelidir.");

        // Her lokasyonun HoM'unu bul (Authentication kurali: her KKK'ya bir HoM atanir).
        var homs = await _users.FindAsync(u => u.Role == UserRole.HeadOfMaster, ct);
        var homByLocation = homs.Where(h => h.LocationId.HasValue)
                                .ToDictionary(h => h.LocationId!.Value, h => h);

        var created = new List<WorkItem>();
        foreach (var item in request.Items)
        {
            if (!homByLocation.TryGetValue(item.LocationId, out var hom))
                return Result<List<WorkItemDto>>.Failure($"Lokasyon {item.LocationId} icin tanimli Head of Master yok.");

            created.Add(new WorkItem
            {
                ReportDate = request.ReportDate.Date,
                LocationId = item.LocationId,
                ProjectId = item.ProjectId,
                SubSubTypeOfWorkId = item.SubSubTypeOfWorkId,
                PlannedQuantity = item.PlannedQuantity,
                PlannedManday = item.PlannedManday,
                SubmittedById = request.SubmittedById,
                AssignedHomId = hom.Id,
                Status = WorkItemStatus.Planned
            });
        }

        await _workItems.AddRangeAsync(created, ct);
        await _workItems.SaveChangesAsync(ct);

        // Detaylariyla tekrar cek ve HoM'lara SignalR bildirimi gonder (T0 adimi).
        var dtos = new List<WorkItemDto>();
        foreach (var w in created)
        {
            var full = await _workItems.GetWithDetailsAsync(w.Id, ct);
            var dto = WorkItemMapper.ToDto(full!);
            dtos.Add(dto);
            await _notifications.NotifyUserAsync(dto.AssignedHomEmail, "PlanAssigned", dto, ct);
        }

        return Result<List<WorkItemDto>>.Success(dtos);
    }
}
