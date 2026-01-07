using System;
using System.Linq;
using System.Security.Claims;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Services;

public class DashboardMetricsService : IDashboardMetricsService
{
    private readonly MedicalOfficeContext _context;

    public DashboardMetricsService(MedicalOfficeContext context)
    {
        _context = context;
    }

    public async Task<LayoutViewModel> GetLayoutViewModelAsync(ClaimsPrincipal user)
    {
        var now = DateTime.UtcNow;

        var upcomingAppointments = await _context.RendezVous
            .AsNoTracking()
            .CountAsync(r => r.DateDebut >= now);

        var totalPatients = await _context.Patients
            .AsNoTracking()
            .CountAsync();

        var openInvoices = await _context.BillingInvoices
            .AsNoTracking()
            .CountAsync(i => i.Status != "Paid");

        var pendingLabResults = await _context.LabResults
            .AsNoTracking()
            .CountAsync(r => r.Status != "Ready" && r.Status != "Completed");

        var pendingAppointmentApprovals = await _context.RendezVous
            .AsNoTracking()
            .CountAsync(r => r.Statut == "Pending Approval");

        return new LayoutViewModel
        {
            UpcomingAppointments = upcomingAppointments,
            TotalPatients = totalPatients,
            OpenInvoices = openInvoices,
            PendingLabResults = pendingLabResults,
            PendingAppointmentApprovals = pendingAppointmentApprovals
        };
    }

    public async Task<DashboardViewModel> GetDashboardViewModelAsync(ClaimsPrincipal user)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var todayEnd = todayStart.AddDays(1);
        var weekEnd = todayStart.AddDays(7);
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        var viewModel = new DashboardViewModel();

        // Common metrics for Admin/Secretary
        if (user.IsInRole(SystemRoles.Admin) || user.IsInRole(SystemRoles.Secretaire))
        {
            viewModel.TotalPatients = await _context.Patients
                .AsNoTracking()
                .CountAsync();

            viewModel.UpcomingAppointments = await _context.RendezVous
                .AsNoTracking()
                .CountAsync(r => r.DateDebut >= now);

            viewModel.TodayAppointments = await _context.RendezVous
                .AsNoTracking()
                .CountAsync(r => r.DateDebut >= todayStart && r.DateDebut < todayEnd);

            viewModel.ThisWeekAppointments = await _context.RendezVous
                .AsNoTracking()
                .CountAsync(r => r.DateDebut >= todayStart && r.DateDebut < weekEnd);

            viewModel.PendingAppointmentApprovals = await _context.RendezVous
                .AsNoTracking()
                .CountAsync(r => r.Statut == "Pending Approval");

            viewModel.OpenInvoices = await _context.BillingInvoices
                .AsNoTracking()
                .CountAsync(i => i.Status != "Paid");

            viewModel.TotalRevenue = await _context.BillingInvoices
                .AsNoTracking()
                .Where(i => i.Status == "Paid")
                .SumAsync(i => (decimal?)i.Amount) ?? 0;

            viewModel.PendingLabResults = await _context.LabResults
                .AsNoTracking()
                .CountAsync(l => l.Status != "Ready" && l.Status != "Completed");

            viewModel.TotalPrescriptions = await _context.Prescriptions
                .AsNoTracking()
                .CountAsync();

            // Recent appointments
            viewModel.UpcomingAppointmentsList = await _context.RendezVous
                .AsNoTracking()
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .Where(r => r.DateDebut >= now)
                .OrderBy(r => r.DateDebut)
                .Take(5)
                .Select(r => new UpcomingAppointmentItem
                {
                    Id = r.Id,
                    DateDebut = r.DateDebut,
                    DateFin = r.DateFin,
                    PatientName = r.Patient.Nom + " " + r.Patient.Prenom,
                    MedecinName = r.Medecin.NomPrenom,
                    Status = r.Statut,
                    Motif = r.Motif
                })
                .ToListAsync();

            // Recent patients
            viewModel.RecentPatients = await _context.Patients
                .AsNoTracking()
                .OrderByDescending(p => p.CreatedAt)
                .Take(5)
                .Select(p => new RecentPatientItem
                {
                    Id = p.Id,
                    FullName = p.Nom + " " + p.Prenom,
                    Email = p.Email,
                    Telephone = p.Telephone,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }
        // Medecin specific metrics
        else if (user.IsInRole(SystemRoles.Medecin) && !string.IsNullOrWhiteSpace(userId))
        {
            var medecin = await _context.Medecins
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ApplicationUserId == userId);

            if (medecin != null)
            {
                var medecinId = medecin.Id;

                viewModel.MyTodayAppointments = await _context.RendezVous
                    .AsNoTracking()
                    .CountAsync(r => r.MedecinId == medecinId && r.DateDebut >= todayStart && r.DateDebut < todayEnd);

                viewModel.MyUpcomingAppointments = await _context.RendezVous
                    .AsNoTracking()
                    .CountAsync(r => r.MedecinId == medecinId && r.DateDebut >= now);

                viewModel.MyPatients = await _context.RendezVous
                    .AsNoTracking()
                    .Where(r => r.MedecinId == medecinId)
                    .Select(r => r.PatientId)
                    .Distinct()
                    .CountAsync();

                viewModel.MyPendingConsultations = await _context.Consultations
                    .AsNoTracking()
                    .CountAsync(c => c.MedecinId == medecinId && c.DateConsult >= now);

                // Upcoming appointments for this medecin
                viewModel.UpcomingAppointmentsList = await _context.RendezVous
                    .AsNoTracking()
                    .Include(r => r.Patient)
                    .Where(r => r.MedecinId == medecinId && r.DateDebut >= now)
                    .OrderBy(r => r.DateDebut)
                    .Take(5)
                    .Select(r => new UpcomingAppointmentItem
                    {
                        Id = r.Id,
                        DateDebut = r.DateDebut,
                        DateFin = r.DateFin,
                        PatientName = r.Patient.Nom + " " + r.Patient.Prenom,
                        MedecinName = medecin.NomPrenom,
                        Status = r.Statut,
                        Motif = r.Motif
                    })
                    .ToListAsync();
            }
        }
        // Patient specific metrics
        else if (user.IsInRole(SystemRoles.Patient) && !string.IsNullOrWhiteSpace(userId))
        {
            var patient = await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            if (patient != null)
            {
                viewModel.PatientProfile = patient;

                viewModel.MyUpcomingAppointmentsCount = await _context.RendezVous
                    .AsNoTracking()
                    .CountAsync(r => r.PatientId == patient.Id && r.DateDebut >= now);

                viewModel.MyLabResultsCount = await _context.LabResults
                    .AsNoTracking()
                    .CountAsync(l => l.PatientId == patient.Id);

                viewModel.MyPrescriptionsCount = await _context.Prescriptions
                    .AsNoTracking()
                    .CountAsync(p => p.PatientId == patient.Id);

                // Upcoming appointments for this patient
                viewModel.UpcomingAppointmentsList = await _context.RendezVous
                    .AsNoTracking()
                    .Include(r => r.Medecin)
                    .Where(r => r.PatientId == patient.Id && r.DateDebut >= now)
                    .OrderBy(r => r.DateDebut)
                    .Take(5)
                    .Select(r => new UpcomingAppointmentItem
                    {
                        Id = r.Id,
                        DateDebut = r.DateDebut,
                        DateFin = r.DateFin,
                        PatientName = patient.Nom + " " + patient.Prenom,
                        MedecinName = r.Medecin.NomPrenom,
                        Status = r.Statut,
                        Motif = r.Motif
                    })
                    .ToListAsync();
            }
        }

        return viewModel;
    }
}
