using System;
using System.Collections.Generic;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
        public int PendingApprovalCount { get; set; }
        public IEnumerable<AppointmentViewModel> Appointments { get; set; } = Array.Empty<AppointmentViewModel>();
        
        // Filtering options
        public string? StatusFilter { get; set; }
        public int? MedecinFilter { get; set; }
        public DateTime? DateFromFilter { get; set; }
        public DateTime? DateToFilter { get; set; }
        public string? SearchTerm { get; set; }
        
        // Select lists for filters
        [ValidateNever]
        public IEnumerable<SelectListItem> StatusOptions { get; set; } = Array.Empty<SelectListItem>();
        
        [ValidateNever]
        public IEnumerable<SelectListItem> MedecinOptions { get; set; } = Array.Empty<SelectListItem>();
    }

    public class ScheduleFormViewModel
    {
        public RendezVou Appointment { get; set; } = new();

        [ValidateNever]
        public IEnumerable<SelectListItem> Patients { get; set; } = Array.Empty<SelectListItem>();

        [ValidateNever]
        public IEnumerable<SelectListItem> Medecins { get; set; } = Array.Empty<SelectListItem>();
    }

    public class PendingApprovalViewModel
    {
        public IEnumerable<AppointmentViewModel> PendingAppointments { get; set; } = Array.Empty<AppointmentViewModel>();
        public int TotalPending { get; set; }
        public int UrgentCount { get; set; } // Appointments requested for today or tomorrow
        
        // Filtering options
        public int? MedecinFilter { get; set; }
        public DateTime? DateFromFilter { get; set; }
        public DateTime? DateToFilter { get; set; }
        public string? SearchTerm { get; set; }
        
        // Select lists for filters
        [ValidateNever]
        public IEnumerable<SelectListItem> MedecinOptions { get; set; } = Array.Empty<SelectListItem>();
    }
}
