// File: ViewModels/Appointments/AppointmentsIndexViewModel.cs
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Appointments
{
    public class AppointmentsIndexViewModel
    {
        public List<AppointmentViewModel> Appointments { get; set; } = new();
        public int TodayCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int WaitingCount { get; set; }
    }
}
