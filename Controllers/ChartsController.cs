using System;
using System.Linq;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text;

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
            var consultations = await _context.Consultations
                .AsNoTracking()
                .Include(c => c.Patient)
                .Include(c => c.Medecin)
                .Include(c => c.RendezVous)
                .OrderByDescending(c => c.DateConsult)
                .ToListAsync();

            var charts = consultations
                .Select(c => new ChartViewModel
                {
                    Id = c.Id,
                    PatientName = c.Patient == null
                        ? "Unknown patient"
                        : $"{c.Patient.Prenom} {c.Patient.Nom}".Trim(),
                    VisitReason = c.RendezVous == null || string.IsNullOrWhiteSpace(c.RendezVous.Motif) ? "Consultation" : c.RendezVous.Motif,
                    Status = string.IsNullOrWhiteSpace(c.Diagnostics) ? "In Review" : "Signed",
                    Provider = c.Medecin == null || string.IsNullOrWhiteSpace(c.Medecin.NomPrenom)
                        ? "Unassigned"
                        : c.Medecin.NomPrenom,
                    LastUpdated = c.DateConsult
                })
                .ToList();

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

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultation = await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Medecin)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null)
            {
                return NotFound();
            }

            return View(consultation);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateLookupsAsync();
            return View(new Consultation { DateConsult = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Consultation consultation)
        {
            // Validate foreign keys exist
            if (consultation.PatientId > 0 && !await _context.Patients.AnyAsync(p => p.Id == consultation.PatientId))
            {
                ModelState.AddModelError(nameof(Consultation.PatientId), "Selected patient does not exist.");
            }

            if (consultation.MedecinId > 0 && !await _context.Medecins.AnyAsync(m => m.Id == consultation.MedecinId))
            {
                ModelState.AddModelError(nameof(Consultation.MedecinId), "Selected provider does not exist.");
            }

            if (consultation.RendezVousId.HasValue && !await _context.RendezVous.AnyAsync(r => r.Id == consultation.RendezVousId.Value))
            {
                ModelState.AddModelError(nameof(Consultation.RendezVousId), "Selected appointment does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return View(consultation);
            }

            _context.Add(consultation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation == null)
            {
                return NotFound();
            }

            await PopulateLookupsAsync();
            return View(consultation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Consultation consultation)
        {
            if (id != consultation.Id)
            {
                return NotFound();
            }

            // Validate foreign keys exist
            if (consultation.PatientId > 0 && !await _context.Patients.AnyAsync(p => p.Id == consultation.PatientId))
            {
                ModelState.AddModelError(nameof(Consultation.PatientId), "Selected patient does not exist.");
            }

            if (consultation.MedecinId > 0 && !await _context.Medecins.AnyAsync(m => m.Id == consultation.MedecinId))
            {
                ModelState.AddModelError(nameof(Consultation.MedecinId), "Selected provider does not exist.");
            }

            if (consultation.RendezVousId.HasValue && !await _context.RendezVous.AnyAsync(r => r.Id == consultation.RendezVousId.Value))
            {
                ModelState.AddModelError(nameof(Consultation.RendezVousId), "Selected appointment does not exist.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return View(consultation);
            }

            try
            {
                _context.Update(consultation);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ConsultationExists(consultation.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var consultation = await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Medecin)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (consultation == null)
            {
                return NotFound();
            }

            return View(consultation);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var consultation = await _context.Consultations.FindAsync(id);
            if (consultation != null)
            {
                _context.Consultations.Remove(consultation);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Medecin)
                .OrderByDescending(c => c.DateConsult)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Id,Patient,Provider,Reason,Status,Date");

            foreach (var c in consultations)
            {
                var patient = c.Patient == null ? "Unknown" : $"{c.Patient.Prenom} {c.Patient.Nom}".Trim();
                var provider = c.Medecin?.NomPrenom ?? "Unassigned";
                var reason = string.IsNullOrWhiteSpace(c.RendezVous?.Motif) ? "Consultation" : c.RendezVous!.Motif;
                var status = string.IsNullOrWhiteSpace(c.Diagnostics) ? "In Review" : "Signed";

                builder.AppendLine($"{c.Id},\"{patient}\",\"{provider}\",\"{reason}\",\"{status}\",{c.DateConsult:O}");
            }

            var bytes = Encoding.UTF8.GetBytes(builder.ToString());
            return File(bytes, "text/csv", "consultations.csv");
        }

        private async Task PopulateLookupsAsync()
        {
            var patients = await _context.Patients
                .AsNoTracking()
                .OrderBy(p => p.Nom)
                .Select(p => new { p.Id, Name = $"{p.Prenom} {p.Nom}".Trim() })
                .ToListAsync();

            var medecins = await _context.Medecins
                .AsNoTracking()
                .OrderBy(m => m.NomPrenom)
                .ToListAsync();

            ViewData["PatientId"] = new SelectList(patients, "Id", "Name");
            ViewData["MedecinId"] = new SelectList(medecins, "Id", "NomPrenom");
        }

        private Task<bool> ConsultationExists(int id)
        {
            return _context.Consultations.AnyAsync(e => e.Id == id);
        }
    }
}
