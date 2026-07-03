using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace WorkforceExecution.WebApi.Hubs;

// Baglanan kullanici e-posta ve rol gruplarina eklenir;
// boylece "kisiye ozel" ve "role ozel" canli bildirim gonderilebilir.
[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var email = Context.User?.FindFirstValue(ClaimTypes.Email);
        var role = Context.User?.FindFirstValue(ClaimTypes.Role);

        if (!string.IsNullOrEmpty(email))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{email}");
        if (!string.IsNullOrEmpty(role))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role:{role}");

        await base.OnConnectedAsync();
    }
}
