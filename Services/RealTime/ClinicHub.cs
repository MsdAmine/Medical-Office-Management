using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using System.Security.Claims;

namespace MedicalOfficeManagement.Services.RealTime
{
    public interface IClinicHubClient
    {
        Task AppointmentCreated(AppointmentUpdateDto update);
        Task AppointmentStatusChanged(AppointmentUpdateDto update);
        Task AppointmentCancelled(AppointmentUpdateDto update);
        Task PatientCheckedIn(PatientCheckInDto update);
        Task InvoiceStatusUpdated(InvoiceStatusUpdateDto update);
        Task DoctorAvailabilityChanged(DoctorAvailabilityUpdateDto update);
    }

    [Authorize]
    public class ClinicHub : Hub<IClinicHubClient>
    {
        public const string ClinicGroup = "Clinic";
        public const string BillingGroup = "Billing";
        public const string DashboardGroup = "Dashboard";
        public const string DoctorGroupPrefix = "Doctor:";

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ClinicGroup);
            await Groups.AddToGroupAsync(Context.ConnectionId, DashboardGroup);

            if (UserIsInRole("Admin") || UserIsInRole("Billing"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, BillingGroup);
            }

            await base.OnConnectedAsync();
        }

        public Task JoinDoctorGroup(string doctorId)
        {
            if (string.IsNullOrWhiteSpace(doctorId))
            {
                return Task.CompletedTask;
            }

            return Groups.AddToGroupAsync(Context.ConnectionId, $"{DoctorGroupPrefix}{doctorId}");
        }

        private bool UserIsInRole(string role) =>
            Context.User?.Claims.Any(c =>
                c.Type == ClaimTypes.Role &&
                string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase)) == true;
    }
}
