using System;
using System.Security.Claims;
using MedicalOfficeManagement.Models;
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

        var pendingLabResults = await _context.ReportArtifacts
            .AsNoTracking()
            .CountAsync(r => r.Status != "Ready");

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

}
