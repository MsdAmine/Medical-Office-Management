using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MedicalOfficeManagement.Services.RealTime
{
    public interface IClinicEventPublisher
    {
        Task PublishAppointmentCreatedAsync(AppointmentUpdateDto update, CancellationToken cancellationToken = default);
        Task PublishAppointmentStatusChangedAsync(AppointmentUpdateDto update, CancellationToken cancellationToken = default);
        Task PublishAppointmentCancelledAsync(AppointmentUpdateDto update, CancellationToken cancellationToken = default);
        Task PublishPatientCheckInAsync(PatientCheckInDto update, CancellationToken cancellationToken = default);
        Task PublishInvoiceStatusUpdatedAsync(InvoiceStatusUpdateDto update, CancellationToken cancellationToken = default);
        Task PublishDoctorAvailabilityChangedAsync(DoctorAvailabilityUpdateDto update, CancellationToken cancellationToken = default);
    }

    public class ClinicEventPublisher : IClinicEventPublisher
    {
        private readonly IHubContext<ClinicHub, IClinicHubClient> _hubContext;

        public ClinicEventPublisher(IHubContext<ClinicHub, IClinicHubClient> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task PublishAppointmentCreatedAsync(AppointmentUpdateDto update, CancellationToken cancellationToken = default) =>
            BroadcastAsync(update, (clients) => clients.AppointmentCreated(update), update.DoctorId, cancellationToken);

        public Task PublishAppointmentStatusChangedAsync(AppointmentUpdateDto update, CancellationToken cancellationToken = default) =>
            BroadcastAsync(update, (clients) => clients.AppointmentStatusChanged(update), update.DoctorId, cancellationToken);

        public Task PublishAppointmentCancelledAsync(AppointmentUpdateDto update, CancellationToken cancellationToken = default) =>
            BroadcastAsync(update, (clients) => clients.AppointmentCancelled(update), update.DoctorId, cancellationToken);

        public Task PublishPatientCheckInAsync(PatientCheckInDto update, CancellationToken cancellationToken = default) =>
            BroadcastAsync(update, (clients) => clients.PatientCheckedIn(update), doctorId: null, cancellationToken);

        public Task PublishInvoiceStatusUpdatedAsync(InvoiceStatusUpdateDto update, CancellationToken cancellationToken = default) =>
            BroadcastAsync(update, (clients) => clients.InvoiceStatusUpdated(update), doctorId: null, cancellationToken, includeBilling: true);

        public Task PublishDoctorAvailabilityChangedAsync(DoctorAvailabilityUpdateDto update, CancellationToken cancellationToken = default) =>
            BroadcastAsync(update, (clients) => clients.DoctorAvailabilityChanged(update), update.DoctorId, cancellationToken);

        private Task BroadcastAsync(
            ClinicEventBase payload,
            Func<IClinicHubClient, Task> send,
            string? doctorId,
            CancellationToken cancellationToken,
            bool includeBilling = false)
        {
            var tasks = new List<Task>
            {
                send(_hubContext.Clients.Group(ClinicHub.ClinicGroup))
            };

            if (payload.AffectedViews?.Any(v => string.Equals(v, "Dashboard", StringComparison.OrdinalIgnoreCase)) == true)
            {
                tasks.Add(send(_hubContext.Clients.Group(ClinicHub.DashboardGroup)));
            }

            if (!string.IsNullOrWhiteSpace(doctorId))
            {
                tasks.Add(send(_hubContext.Clients.Group($"{ClinicHub.DoctorGroupPrefix}{doctorId}")));
            }

            if (includeBilling || payload.AffectedViews?.Any(v => string.Equals(v, "Billing", StringComparison.OrdinalIgnoreCase)) == true)
            {
                tasks.Add(send(_hubContext.Clients.Group(ClinicHub.BillingGroup)));
            }

            return Task.WhenAll(tasks.Append(Task.CompletedTask));
        }
    }
}
