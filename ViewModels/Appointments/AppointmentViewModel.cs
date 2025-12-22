// File: ViewModels/Appointments/AppointmentViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Appointments
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
    }
}