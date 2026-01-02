using System;
using System.Collections.Generic;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalOfficeManagement.ViewModels
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedecinName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
    }

    public class ScheduleIndexViewModel
    {
        public int UpcomingCount { get; set; }
        public int InClinicToday { get; set; }
        public int CompletedThisWeek { get; set; }
        public IEnumerable<AppointmentViewModel> Appointments { get; set; } = Array.Empty<AppointmentViewModel>();
    }

    public class ScheduleFormViewModel
    {
        public RendezVou Appointment { get; set; } = new();
        public IEnumerable<SelectListItem> Patients { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<SelectListItem> Medecins { get; set; } = Array.Empty<SelectListItem>();
    }
}
