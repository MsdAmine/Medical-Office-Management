using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using MedicalOfficeManagement.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Models;
using Microsoft.Extensions.Logging;
using X.PagedList;


namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.AdminOrMedecin)]
    public class PrescriptionsController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly ILogger<PrescriptionsController> _logger;
        private const int PageSize = 20;

        public PrescriptionsController(MedicalOfficeContext context, ILogger<PrescriptionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            _logger.LogInformation("Prescriptions Index accessed by user {UserId}, page {Page}", User.Identity?.Name, page);

            var query = _context.Prescriptions
                .AsNoTracking()
                .Include(p => p.Patient)
                .Include(p => p.Medecin)
                .OrderByDescending(p => p.IssuedOn);

            // Get total count
            var totalCount = await query.CountAsync();
            
            // Get items for current page
            var items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Get all prescriptions for summary statistics
            var allPrescriptions = await _context.Prescriptions
                .AsNoTracking()
                .ToListAsync();

            var mapped = items.Select(p => new PrescriptionViewModel
            {
                Id = p.Id,
                PatientName = p.Patient == null ? "Unknown patient" : $"{p.Patient.Prenom} {p.Patient.Nom}".Trim(),
                Medication = p.Medication,
                Dosage = p.Dosage ?? string.Empty,
                Frequency = p.Frequency ?? string.Empty,
                PrescribedBy = p.Medecin?.NomPrenom ?? "Unassigned",
                IssuedOn = p.IssuedOn,
                NextRefill = p.NextRefill,
                RefillsRemaining = p.RefillsRemaining,
                Status = p.Status,
                Notes = p.Notes ?? string.Empty
            }).ToList();

            // Create IPagedList manually
            var pagedPrescriptions = new StaticPagedList<PrescriptionViewModel>(
                mapped,
                page,
                PageSize,
                totalCount);

            var viewModel = new PrescriptionIndexViewModel
            {
                ActiveCount = allPrescriptions.Count(p => p.StatusEnum == PrescriptionStatus.Active),
                PendingCount = allPrescriptions.Count(p => p.StatusEnum == PrescriptionStatus.Pending),
                CompletedCount = allPrescriptions.Count(p => p.StatusEnum == PrescriptionStatus.Completed),
                Prescriptions = mapped
            };

            ViewData["PageNumber"] = pagedPrescriptions.PageNumber;
            ViewData["PageCount"] = pagedPrescriptions.PageCount;
            ViewData["TotalItemCount"] = pagedPrescriptions.TotalItemCount;
            ViewData["HasPreviousPage"] = pagedPrescriptions.HasPreviousPage;
            ViewData["HasNextPage"] = pagedPrescriptions.HasNextPage;

            ViewData["Title"] = "Prescriptions";
            ViewData["Breadcrumb"] = "Prescriptions";

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Medecin)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateLookupsAsync();
            return View(new Prescription { IssuedOn = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Prescription prescription)
        {
            _logger.LogInformation("Creating prescription for patient {PatientId} by user {UserId}", prescription.PatientId, User.Identity?.Name);

            // Validate foreign keys exist
            if (prescription.PatientId > 0 && !await _context.Patients.AnyAsync(p => p.Id == prescription.PatientId))
            {
                ModelState.AddModelError(nameof(Prescription.PatientId), "Selected patient does not exist.");
            }

            if (prescription.MedecinId.HasValue && prescription.MedecinId.Value > 0 && !await _context.Medecins.AnyAsync(m => m.Id == prescription.MedecinId.Value))
            {
                ModelState.AddModelError(nameof(Prescription.MedecinId), "Selected provider does not exist.");
            }

            // Ensure default status if not set
            if (string.IsNullOrWhiteSpace(prescription.Status))
            {
                prescription.StatusEnum = PrescriptionStatus.Pending;
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Prescription creation validation failed");
                await PopulateLookupsAsync();
                return View(prescription);
            }

            try
            {
                _context.Prescriptions.Add(prescription);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Prescription {PrescriptionId} created successfully", prescription.Id);
                TempData["StatusMessage"] = "Prescription created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the prescription. Please try again.");
                await PopulateLookupsAsync();
                return View(prescription);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                return NotFound();
            }

            await PopulateLookupsAsync();
            return View(prescription);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Prescription prescription)
        {
            _logger.LogInformation("Editing prescription {PrescriptionId} by user {UserId}", id, User.Identity?.Name);

            if (id != prescription.Id)
            {
                return NotFound();
            }

            // Validate foreign keys exist
            if (prescription.PatientId > 0 && !await _context.Patients.AnyAsync(p => p.Id == prescription.PatientId))
            {
                ModelState.AddModelError(nameof(Prescription.PatientId), "Selected patient does not exist.");
            }

            if (prescription.MedecinId.HasValue && prescription.MedecinId.Value > 0 && !await _context.Medecins.AnyAsync(m => m.Id == prescription.MedecinId.Value))
            {
                ModelState.AddModelError(nameof(Prescription.MedecinId), "Selected provider does not exist.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Prescription {PrescriptionId} edit validation failed", id);
                await PopulateLookupsAsync();
                return View(prescription);
            }

            try
            {
                _context.Update(prescription);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Prescription {PrescriptionId} updated successfully", id);
                TempData["StatusMessage"] = "Prescription updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PrescriptionExists(prescription.Id))
                {
                    _logger.LogWarning("Prescription {PrescriptionId} not found during edit", id);
                    return NotFound();
                }

                _logger.LogError("Concurrency exception while editing prescription {PrescriptionId}", id);
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

            var prescription = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Medecin)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (prescription == null)
            {
                return NotFound();
            }

            return View(prescription);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription == null)
            {
                _logger.LogWarning("Attempted to delete non-existent prescription with ID {PrescriptionId}", id);
                return NotFound();
            }

            try
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Prescription {PrescriptionId} deleted successfully", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription {PrescriptionId}", id);
                TempData["StatusMessage"] = "Error: Failed to delete prescription. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var prescriptions = await _context.Prescriptions
                .Include(p => p.Patient)
                .Include(p => p.Medecin)
                .OrderByDescending(p => p.IssuedOn)
                .ToListAsync();

            var csv = "Id,Patient,Medication,Dosage,Frequency,Prescriber,Status,IssuedOn,NextRefill,RefillsRemaining\n";
            foreach (var p in prescriptions)
            {
                var patient = p.Patient == null ? "Unknown" : $"{p.Patient.Prenom} {p.Patient.Nom}".Trim();
                var prescriber = p.Medecin?.NomPrenom ?? "Unassigned";
                csv += $"{p.Id},\"{patient}\",\"{p.Medication}\",\"{p.Dosage}\",\"{p.Frequency}\",\"{prescriber}\",\"{p.Status}\",{p.IssuedOn:O},{p.NextRefill:O},{p.RefillsRemaining}\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "prescriptions.csv");
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

        private Task<bool> PrescriptionExists(int id)
        {
            return _context.Prescriptions.AnyAsync(e => e.Id == id);
        }
    }
}
