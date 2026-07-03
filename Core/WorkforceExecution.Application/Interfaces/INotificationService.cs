namespace WorkforceExecution.Application.Interfaces;

// SignalR implementasyonu WebApi katmanindadir; Application sadece sozlesmeyi bilir.
public interface INotificationService
{
    Task NotifyUserAsync(string userEmail, string eventName, object payload, CancellationToken ct = default);
    Task NotifyRoleAsync(string roleGroup, string eventName, object payload, CancellationToken ct = default);
    Task BroadcastAsync(string eventName, object payload, CancellationToken ct = default);
}
