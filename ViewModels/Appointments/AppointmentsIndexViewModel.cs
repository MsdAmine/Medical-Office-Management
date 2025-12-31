// File: ViewModels/Appointments/AppointmentsIndexViewModel.cs
using System.Collections.Generic;
using MedicalOfficeManagement.ViewModels.Filters;

namespace MedicalOfficeManagement.ViewModels.Appointments
{
    public class AppointmentsIndexViewModel
    {
        public List<AppointmentViewModel> Appointments { get; set; } = new();
        public int TodayCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int WaitingCount { get; set; }
        public FilterContextViewModel<AppointmentsFilterCriteria> FilterContext { get; set; } = default!;
        public List<string> DoctorOptions { get; set; } = new();
        public List<string> StatusOptions { get; set; } = new();
    }
}
