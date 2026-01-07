using MedicalOfficeManagement.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MedicalOfficeManagement.Services
{
    public interface INotificationService
    {
        Task SendNotificationToUserAsync(string userId, string title, string message, string type = "info", string? url = null);
        Task SendNotificationToRoleAsync(string role, string title, string message, string type = "info", string? url = null);
        Task SendNotificationToAllAsync(string title, string message, string type = "info", string? url = null);
    }

    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task SendNotificationToUserAsync(string userId, string title, string message, string type = "info", string? url = null)
        {
            try
            {
                var notification = new NotificationMessage
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Url = url,
                    Timestamp = DateTime.Now
                };

                await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("Notification sent to user {UserId}: {Title}", userId, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
            }
        }

        public async Task SendNotificationToRoleAsync(string role, string title, string message, string type = "info", string? url = null)
        {
            try
            {
                var notification = new NotificationMessage
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Url = url,
                    Timestamp = DateTime.Now
                };

                await _hubContext.Clients.Group(role.ToLower() + "s").SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("Notification sent to role {Role}: {Title}", role, title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to role {Role}", role);
            }
        }

        public async Task SendNotificationToAllAsync(string title, string message, string type = "info", string? url = null)
        {
            try
            {
                var notification = new NotificationMessage
                {
                    Title = title,
                    Message = message,
                    Type = type,
                    Url = url,
                    Timestamp = DateTime.Now
                };

                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
                _logger.LogInformation("Notification sent to all users: {Title}", title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to all users");
            }
        }
    }
}
