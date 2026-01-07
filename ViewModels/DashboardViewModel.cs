using System;
using System.Collections.Generic;
using MedicalOfficeManagement.Models;

namespace MedicalOfficeManagement.ViewModels
{
    public class DashboardViewModel
    {
        // Common metrics
        public int TotalPatients { get; set; }
        public int UpcomingAppointments { get; set; }
        public int PendingAppointmentApprovals { get; set; }
        public int OpenInvoices { get; set; }
        public int PendingLabResults { get; set; }

        // Admin/Secretary specific
        public int TodayAppointments { get; set; }
        public int ThisWeekAppointments { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalPrescriptions { get; set; }

        // Medecin specific
        public int MyTodayAppointments { get; set; }
        public int MyUpcomingAppointments { get; set; }
        public int MyPatients { get; set; }
        public int MyPendingConsultations { get; set; }

        // Patient specific
        public Patient? PatientProfile { get; set; }
        public int MyUpcomingAppointmentsCount { get; set; }
        public int MyLabResultsCount { get; set; }
        public int MyPrescriptionsCount { get; set; }

        // Recent items
        public List<UpcomingAppointmentItem> UpcomingAppointmentsList { get; set; } = new();
        public List<RecentPatientItem> RecentPatients { get; set; } = new();
    }

    public class UpcomingAppointmentItem
    {
        public int Id { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedecinName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Motif { get; set; }
    }

    public class RecentPatientItem
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
