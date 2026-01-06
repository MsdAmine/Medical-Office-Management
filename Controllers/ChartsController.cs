using System;
using System.Linq;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.AdminOrMedecin)]
    public class ChartsController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly ILogger<ChartsController> _logger;

        public ChartsController(MedicalOfficeContext context, ILogger<ChartsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var charts = await _context.Consultations
                .AsNoTracking()
                .Include(c => c.Patient)
                .Include(c => c.Medecin)
                .Include(c => c.RendezVous)
                .OrderByDescending(c => c.DateConsult)
                .Take(15)
                .Select(c => new ChartViewModel
                {
                    PatientName = c.Patient == null
                        ? "Unknown patient"
                        : $"{c.Patient.Prenom} {c.Patient.Nom}".Trim(),
                    VisitReason = string.IsNullOrWhiteSpace(c.RendezVous?.Motif) ? "Consultation" : c.RendezVous.Motif,
                    Status = string.IsNullOrWhiteSpace(c.Diagnostics) ? "In Review" : "Signed",
                    Provider = c.Medecin == null || string.IsNullOrWhiteSpace(c.Medecin.NomPrenom)
                        ? "Unassigned"
                        : c.Medecin.NomPrenom,
                    LastUpdated = c.DateConsult
                })
                .ToListAsync();

            var viewModel = new ChartIndexViewModel
            {
                ActiveCharts = charts.Count,
                PendingSignoffs = charts.Count(c => !string.Equals(c.Status, "Signed", StringComparison.OrdinalIgnoreCase)),
                CriticalAlerts = await _context.Consultations.AsNoTracking()
                    .CountAsync(c => !string.IsNullOrWhiteSpace(c.Diagnostics) && EF.Functions.Like(c.Diagnostics!, "%critical%")),
                StatusMessage = charts.Any() ? null : "No clinical documentation found in the system.",
                Charts = charts
            };

            ViewData["Title"] = "Medical Charts";
            ViewData["Breadcrumb"] = "Clinical";

            return View(viewModel);
        }
    }
}
