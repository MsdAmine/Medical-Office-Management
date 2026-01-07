using System;
using System.ComponentModel.DataAnnotations;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalOfficeManagement.ViewModels
{
    public class PatientAppointmentViewModel
    {
        public int Id { get; set; }

        public DateTime ScheduledFor { get; set; }

        public DateTime EndsAt { get; set; }

        public string MedecinName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string Reason { get; set; } = string.Empty;
    }

    public class PatientTreatmentViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string PrescribedBy { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string Notes { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }

    public class PatientResultViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime Date { get; set; }

        public string OrderedBy { get; set; } = string.Empty;

        public string Notes { get; set; } = string.Empty;
    }

    public class AppointmentRequestInput
    {
        [Required]
        [Display(Name = "Clinician")]
        public int MedecinId { get; set; }

        [Required]
        [Display(Name = "Preferred start")]
        public DateTime PreferredStart { get; set; }

        [Required]
        [Display(Name = "Preferred end")]
        public DateTime PreferredEnd { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Reason for visit")]
        public string Reason { get; set; } = string.Empty;
    }

    public class PatientPortalViewModel
    {
        public Patient? PatientProfile { get; set; }

        public IEnumerable<PatientAppointmentViewModel> UpcomingAppointments { get; set; } = Array.Empty<PatientAppointmentViewModel>();

        public IEnumerable<PatientAppointmentViewModel> PastAppointments { get; set; } = Array.Empty<PatientAppointmentViewModel>();

        public IEnumerable<PatientTreatmentViewModel> Treatments { get; set; } = Array.Empty<PatientTreatmentViewModel>();

        public IEnumerable<PatientResultViewModel> LabResults { get; set; } = Array.Empty<PatientResultViewModel>();

        public AppointmentRequestInput Request { get; set; } = new();

        public IEnumerable<SelectListItem> MedecinOptions { get; set; } = Array.Empty<SelectListItem>();

        public string? StatusMessage { get; set; }

        public bool HasProfile => PatientProfile != null;

        public string PatientDisplayName
        {
            get
            {
                if (PatientProfile == null)
                    return "Patient";

                var fullName = $"{PatientProfile.Prenom} {PatientProfile.Nom}".Trim();
                return string.IsNullOrWhiteSpace(fullName) ? "Patient" : fullName;
            }
        }
    }

    public class TimelineItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty; // "appointment", "consultation", "lab", "prescription", "billing"
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? LinkUrl { get; set; }
    }

    public class TimelineViewModel
    {
        public Patient? PatientProfile { get; set; }
        public IEnumerable<TimelineItemViewModel> TimelineItems { get; set; } = Array.Empty<TimelineItemViewModel>();
    }
}
