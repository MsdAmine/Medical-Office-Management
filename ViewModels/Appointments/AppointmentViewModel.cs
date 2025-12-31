// File: ViewModels/Appointments/AppointmentViewModel.cs
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Appointments
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }
        public DateTime EndTime { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public string StatusIcon { get; set; } = string.Empty;
        public string VisitType { get; set; } = string.Empty;
        public string DurationLabel { get; set; } = string.Empty;
        public string Room { get; set; } = string.Empty;
        public bool IsLate { get; set; }
        public bool HasOverlap { get; set; }
        public string PatientLastVisitRelative { get; set; } = "—";
        public string PatientUpcoming { get; set; } = "—";
        public List<string> PatientRiskFlags { get; set; } = new();
    }
}
