// File: ViewModels/Appointments/AppointmentViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Appointments
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
    }
}
