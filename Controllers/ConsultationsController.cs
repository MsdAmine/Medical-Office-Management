using System;
using System.Linq;
using System.Threading.Tasks;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using X.PagedList;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.ClinicalTeam)]
    public class ConsultationsController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly ILogger<ConsultationsController> _logger;

        public ConsultationsController(MedicalOfficeContext context, ILogger<ConsultationsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 20;

            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                return Forbid();
            }

            var query = GetScopedConsultationsQueryable(medecinId)
                .OrderByDescending(c => c.DateConsult);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pagedConsultations = new StaticPagedList<Consultation>(
                items,
                page,
                pageSize,
                totalCount);

            ViewData["Title"] = "Consultations";
            ViewData["Breadcrumb"] = "Consultations";
            ViewData["PageNumber"] = pagedConsultations.PageNumber;
            ViewData["PageCount"] = pagedConsultations.PageCount;
            ViewData["TotalItemCount"] = pagedConsultations.TotalItemCount;
            ViewData["HasPreviousPage"] = pagedConsultations.HasPreviousPage;
            ViewData["HasNextPage"] = pagedConsultations.HasNextPage;

            return View(pagedConsultations);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var consultation = await BuildConsultationQuery()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null)
                return NotFound();

            if (!await CanAccessConsultationAsync(consultation))
                return Forbid();

            ViewData["Title"] = "Consultation Details";
            ViewData["Breadcrumb"] = "Consultations";
            return View(consultation);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? rendezVousId)
        {
            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                return Forbid();
            }

            var consultation = new Consultation
            {
                DateConsult = DateTime.Now,
                RendezVousId = rendezVousId
            };

            // Pre-fill from appointment if provided
            if (rendezVousId.HasValue)
            {
                var appointment = await _context.RendezVous
                    .Include(r => r.Patient)
                    .Include(r => r.Medecin)
                    .FirstOrDefaultAsync(r => r.Id == rendezVousId.Value);

                if (appointment != null)
                {
                    consultation.PatientId = appointment.PatientId;
                    consultation.MedecinId = appointment.MedecinId;
                    consultation.DateConsult = appointment.DateDebut;
                }
            }

            ViewBag.Patients = await GetPatientsSelectListAsync(consultation.PatientId, medecinId);
            ViewBag.Medecins = await GetMedecinsSelectListAsync(consultation.MedecinId, medecinId);
            ViewBag.RendezVousId = rendezVousId;

            ViewData["Title"] = "New Consultation";
            ViewData["Breadcrumb"] = "Consultations";
            return View(consultation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Consultation consultation)
        {
            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                return Forbid();
            }

            if (User.IsInRole(SystemRoles.Medecin))
            {
                consultation.MedecinId = medecinId.Value;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Patients = await GetPatientsSelectListAsync(consultation.PatientId, medecinId);
                ViewBag.Medecins = await GetMedecinsSelectListAsync(consultation.MedecinId, medecinId);
                ViewBag.RendezVousId = consultation.RendezVousId;
                ViewData["Title"] = "New Consultation";
                ViewData["Breadcrumb"] = "Consultations";
                return View(consultation);
            }

            try
            {
                _context.Consultations.Add(consultation);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Consultation {ConsultationId} created successfully for patient {PatientId}",
                    consultation.Id, consultation.PatientId);

                TempData["StatusMessage"] = "Consultation created successfully.";
                
                if (consultation.RendezVousId.HasValue)
                {
                    return RedirectToAction("Details", "Appointments", new { id = consultation.RendezVousId });
                }
                
                return RedirectToAction(nameof(Details), new { id = consultation.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating consultation");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the consultation. Please try again.");
                ViewBag.Patients = await GetPatientsSelectListAsync(consultation.PatientId, medecinId);
                ViewBag.Medecins = await GetMedecinsSelectListAsync(consultation.MedecinId, medecinId);
                ViewBag.RendezVousId = consultation.RendezVousId;
                ViewData["Title"] = "New Consultation";
                ViewData["Breadcrumb"] = "Consultations";
                return View(consultation);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var consultation = await _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Medecin)
                .Include(c => c.RendezVous)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null)
                return NotFound();

            if (!await CanAccessConsultationAsync(consultation))
                return Forbid();

            var medecinId = await GetCurrentMedecinIdAsync();
            ViewBag.Patients = await GetPatientsSelectListAsync(consultation.PatientId, medecinId);
            ViewBag.Medecins = await GetMedecinsSelectListAsync(consultation.MedecinId, medecinId);

            ViewData["Title"] = "Edit Consultation";
            ViewData["Breadcrumb"] = "Consultations";
            return View(consultation);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Consultation consultation)
        {
            if (id != consultation.Id)
                return NotFound();

            var existingConsultation = await _context.Consultations.FindAsync(id);
            if (existingConsultation == null)
                return NotFound();

            if (!await CanAccessConsultationAsync(existingConsultation))
                return Forbid();

            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin))
            {
                consultation.MedecinId = medecinId.Value;
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Patients = await GetPatientsSelectListAsync(consultation.PatientId, medecinId);
                ViewBag.Medecins = await GetMedecinsSelectListAsync(consultation.MedecinId, medecinId);
                ViewData["Title"] = "Edit Consultation";
                ViewData["Breadcrumb"] = "Consultations";
                return View(consultation);
            }

            try
            {
                existingConsultation.PatientId = consultation.PatientId;
                existingConsultation.MedecinId = consultation.MedecinId;
                existingConsultation.RendezVousId = consultation.RendezVousId;
                existingConsultation.DateConsult = consultation.DateConsult;
                existingConsultation.Observations = consultation.Observations;
                existingConsultation.Diagnostics = consultation.Diagnostics;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Consultation {ConsultationId} updated successfully", id);
                TempData["StatusMessage"] = "Consultation updated successfully.";
                return RedirectToAction(nameof(Details), new { id = consultation.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsultationExists(consultation.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GenerateInvoice(int id)
        {
            var consultation = await BuildConsultationQuery()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (consultation == null)
                return NotFound();

            if (!await CanAccessConsultationAsync(consultation))
                return Forbid();

            // Check if invoice already exists
            var existingInvoice = await _context.BillingInvoices
                .FirstOrDefaultAsync(i => i.ConsultationId == id);

            if (existingInvoice != null)
            {
                TempData["StatusMessage"] = "An invoice already exists for this consultation.";
                return RedirectToAction("Details", "Billing", new { id = existingInvoice.Id });
            }

            var patient = consultation.Patient;
            var patientName = patient != null ? $"{patient.Prenom} {patient.Nom}".Trim() : "Unknown Patient";

            return RedirectToAction("Create", "Billing", new { 
                consultationId = consultation.Id,
                patientId = consultation.PatientId,
                service = $"Consultation - {consultation.DateConsult:yyyy-MM-dd}",
                amount = 0
            });
        }

        private IQueryable<Consultation> BuildConsultationQuery()
        {
            return _context.Consultations
                .Include(c => c.Patient)
                .Include(c => c.Medecin)
                .Include(c => c.RendezVous);
        }

        private IQueryable<Consultation> GetScopedConsultationsQueryable(int? medecinId)
        {
            var query = BuildConsultationQuery();

            if (IsAdminOrSecretaire())
            {
                return query;
            }

            if (User.IsInRole(SystemRoles.Medecin) && medecinId.HasValue)
            {
                return query.Where(c => c.MedecinId == medecinId.Value);
            }

            return query.Where(_ => false);
        }

        private async Task<bool> CanAccessConsultationAsync(Consultation consultation)
        {
            if (IsAdminOrSecretaire())
                return true;

            if (User.IsInRole(SystemRoles.Medecin))
            {
                var medecinId = await GetCurrentMedecinIdAsync();
                return medecinId.HasValue && consultation.MedecinId == medecinId.Value;
            }

            return false;
        }

        private bool IsAdminOrSecretaire()
        {
            return User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Secretaire);
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<int?> GetCurrentMedecinIdAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var medecin = await _context.Medecins
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ApplicationUserId == userId);

            return medecin?.Id;
        }

        private async Task<IEnumerable<SelectListItem>> GetPatientsSelectListAsync(int? selectedId = null, int? medecinId = null)
        {
            var patients = await GetScopedPatientsAsync(medecinId);

            return patients.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Prenom} {p.Nom}".Trim(),
                Selected = selectedId.HasValue && p.Id == selectedId
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetMedecinsSelectListAsync(int? selectedId = null, int? medecinId = null)
        {
            if (IsAdminOrSecretaire())
            {
                var medecins = await _context.Medecins
                    .OrderBy(m => m.NomPrenom)
                    .ToListAsync();

                return medecins.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = m.NomPrenom ?? "Unnamed",
                    Selected = selectedId.HasValue && m.Id == selectedId
                });
            }

            if (User.IsInRole(SystemRoles.Medecin) && medecinId.HasValue)
            {
                var medecin = await _context.Medecins.FindAsync(medecinId.Value);
                if (medecin != null)
                {
                    return new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value = medecin.Id.ToString(),
                            Text = medecin.NomPrenom ?? "Unnamed",
                            Selected = true
                        }
                    };
                }
            }

            return Enumerable.Empty<SelectListItem>();
        }

        private async Task<List<Patient>> GetScopedPatientsAsync(int? medecinId)
        {
            if (IsAdminOrSecretaire())
            {
                return await _context.Patients
                    .OrderBy(p => p.Nom)
                    .ThenBy(p => p.Prenom)
                    .ToListAsync();
            }

            if (User.IsInRole(SystemRoles.Medecin) && medecinId.HasValue)
            {
                return await _context.Patients
                    .Where(p =>
                        _context.RendezVous.Any(r => r.PatientId == p.Id && r.MedecinId == medecinId.Value) ||
                        _context.Consultations.Any(c => c.PatientId == p.Id && c.MedecinId == medecinId.Value))
                    .OrderBy(p => p.Nom)
                    .ThenBy(p => p.Prenom)
                    .ToListAsync();
            }

            return new List<Patient>();
        }

        private bool ConsultationExists(int id)
        {
            return _context.Consultations.Any(e => e.Id == id);
        }

        private string GenerateInvoiceNumber()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");
            var random = new Random().Next(1000, 9999);
            return $"INV-{year}{month}{day}-{random}";
        }
    }
}
