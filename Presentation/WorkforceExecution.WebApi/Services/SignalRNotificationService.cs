using Microsoft.AspNetCore.SignalR;
using WorkforceExecution.Application.Interfaces;
using WorkforceExecution.WebApi.Hubs;

namespace WorkforceExecution.WebApi.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationHub> hubContext) => _hubContext = hubContext;

    public Task NotifyUserAsync(string userEmail, string eventName, object payload, CancellationToken ct = default)
        => _hubContext.Clients.Group($"user:{userEmail}").SendAsync(eventName, payload, ct);

    public Task NotifyRoleAsync(string roleGroup, string eventName, object payload, CancellationToken ct = default)
        => _hubContext.Clients.Group($"role:{roleGroup}").SendAsync(eventName, payload, ct);

    public Task BroadcastAsync(string eventName, object payload, CancellationToken ct = default)
        => _hubContext.Clients.All.SendAsync(eventName, payload, ct);
}
