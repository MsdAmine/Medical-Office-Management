using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MedicalOfficeManagement.Hubs
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            // Add to role-based groups
            if (Context.User?.IsInRole("Admin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            }
            if (Context.User?.IsInRole("Secretaire") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "secretaries");
            }
            if (Context.User?.IsInRole("Medecin") == true)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "doctors");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }

    public class NotificationMessage
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = "info"; // info, success, warning, error
        public string? Url { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
