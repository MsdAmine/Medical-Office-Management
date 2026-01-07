using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Services.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedicalOfficeManagement.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour
        private readonly TimeSpan _reminderWindow = TimeSpan.FromHours(24); // Send reminders 24 hours before

        public AppointmentReminderService(
            IServiceProvider serviceProvider,
            ILogger<AppointmentReminderService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Reminder Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendRemindersAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while sending appointment reminders");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Appointment Reminder Service stopped");
        }

        private async Task SendRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MedicalOfficeContext>();
            var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var now = DateTime.Now;
            var reminderStartTime = now.Add(_reminderWindow);
            var reminderEndTime = reminderStartTime.AddHours(1); // 1 hour window

            // Find appointments that need reminders
            var appointmentsToRemind = await context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .Where(r => r.StatusEnum == AppointmentStatus.Scheduled &&
                            r.DateDebut >= reminderStartTime &&
                            r.DateDebut < reminderEndTime &&
                            !string.IsNullOrWhiteSpace(r.Patient!.Email))
                .ToListAsync(cancellationToken);

            _logger.LogInformation("Found {Count} appointments to send reminders for", appointmentsToRemind.Count);

            foreach (var appointment in appointmentsToRemind)
            {
                try
                {
                    var reminderEmail = AppointmentEmailTemplates.AppointmentReminder(appointment);
                    await emailSender.SendAsync(
                        appointment.Patient!.Email!,
                        "Appointment Reminder",
                        reminderEmail
                    );

                    _logger.LogInformation(
                        "Reminder email sent successfully for appointment {AppointmentId} to {Email}",
                        appointment.Id,
                        appointment.Patient.Email
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to send reminder email for appointment {AppointmentId} to {Email}",
                        appointment.Id,
                        appointment.Patient?.Email
                    );
                }
            }
        }
    }
}
