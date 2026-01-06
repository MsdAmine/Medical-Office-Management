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

        var upcomingAppointmentsTask = _context.RendezVous
            .AsNoTracking()
            .CountAsync(r => r.DateDebut >= now);

        var totalPatientsTask = _context.Patients
            .AsNoTracking()
            .CountAsync();

        var openInvoicesTask = _context.BillingInvoices
            .AsNoTracking()
            .CountAsync(i => !i.Status.Equals("Paid", StringComparison.OrdinalIgnoreCase));

        var pendingLabResultsTask = _context.ReportArtifacts
            .AsNoTracking()
            .CountAsync(r => !r.Status.Equals("Ready", StringComparison.OrdinalIgnoreCase));

        await Task.WhenAll(upcomingAppointmentsTask, totalPatientsTask, openInvoicesTask, pendingLabResultsTask);

        return new LayoutViewModel
        {
            UpcomingAppointments = upcomingAppointmentsTask.Result,
            TotalPatients = totalPatientsTask.Result,
            OpenInvoices = openInvoicesTask.Result,
            PendingLabResults = pendingLabResultsTask.Result
        };
    }
}
