using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using MedicalOfficeManagement.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using MedicalOfficeManagement.Models;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.ClinicalTeam)]
    public class LabsController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly ILogger<LabsController> _logger;
        private const int PageSize = 20;

        public LabsController(MedicalOfficeContext context, ILogger<LabsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            _logger.LogInformation("Lab Results Index accessed by user {UserId}, page {Page}", User.Identity?.Name, page);

            var query = _context.LabResults
                .AsNoTracking()
                .Include(l => l.Patient)
                .Include(l => l.Medecin)
                .OrderByDescending(l => l.CollectedOn);

            // Get total count
            var totalCount = await query.CountAsync();
            
            // Get items for current page
            var items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Get all results for summary statistics
            var allResults = await _context.LabResults
                .AsNoTracking()
                .ToListAsync();

            var mapped = items.Select(r => new LabResultViewModel
            {
                Id = r.Id,
                TestName = r.TestName,
                PatientName = r.Patient == null ? "Unknown patient" : $"{r.Patient.Prenom} {r.Patient.Nom}".Trim(),
                Priority = r.Priority,
                Status = r.Status,
                CollectedOn = r.CollectedOn
            }).ToList();

            // Create IPagedList manually
            var pagedResults = new StaticPagedList<LabResultViewModel>(
                mapped,
                page,
                PageSize,
                totalCount);

            var viewModel = new LabIndexViewModel
            {
                PendingResults = allResults.Count(r => r.StatusEnum == LabResultStatus.Pending || r.StatusEnum == LabResultStatus.InProgress),
                CriticalFindings = allResults.Count(r => r.PriorityEnum == LabResultPriority.Stat),
                CompletedToday = allResults.Count(r => r.CollectedOn.Date == DateTime.UtcNow.Date && r.StatusEnum == LabResultStatus.Completed),
                Results = mapped
            };

            ViewData["PageNumber"] = pagedResults.PageNumber;
            ViewData["PageCount"] = pagedResults.PageCount;
            ViewData["TotalItemCount"] = pagedResults.TotalItemCount;
            ViewData["HasPreviousPage"] = pagedResults.HasPreviousPage;
            ViewData["HasNextPage"] = pagedResults.HasNextPage;

            ViewData["Title"] = "Lab Results";
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

            var result = await _context.LabResults
                .Include(l => l.Patient)
                .Include(l => l.Medecin)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (result == null)
            {
                return NotFound();
            }

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateLookupsAsync();
            return View(new LabResult { CollectedOn = DateTime.UtcNow });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LabResult labResult)
        {
            _logger.LogInformation("Creating lab result for patient {PatientId} by user {UserId}", labResult.PatientId, User.Identity?.Name);

            // Validate foreign keys exist
            if (labResult.PatientId > 0 && !await _context.Patients.AnyAsync(p => p.Id == labResult.PatientId))
            {
                ModelState.AddModelError(nameof(LabResult.PatientId), "Selected patient does not exist.");
            }

            if (labResult.MedecinId.HasValue && labResult.MedecinId.Value > 0 && !await _context.Medecins.AnyAsync(m => m.Id == labResult.MedecinId.Value))
            {
                ModelState.AddModelError(nameof(LabResult.MedecinId), "Selected provider does not exist.");
            }

            // Ensure default values if not set
            if (string.IsNullOrWhiteSpace(labResult.Status))
            {
                labResult.StatusEnum = LabResultStatus.Pending;
            }
            if (string.IsNullOrWhiteSpace(labResult.Priority))
            {
                labResult.PriorityEnum = LabResultPriority.Routine;
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Lab result creation validation failed");
                await PopulateLookupsAsync();
                return View(labResult);
            }

            try
            {
                _context.LabResults.Add(labResult);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Lab result {LabResultId} created successfully", labResult.Id);
                TempData["StatusMessage"] = "Lab result created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating lab result");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the lab result. Please try again.");
                await PopulateLookupsAsync();
                return View(labResult);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var labResult = await _context.LabResults.FindAsync(id);
            if (labResult == null)
            {
                return NotFound();
            }

            await PopulateLookupsAsync();
            return View(labResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LabResult labResult)
        {
            _logger.LogInformation("Editing lab result {LabResultId} by user {UserId}", id, User.Identity?.Name);

            if (id != labResult.Id)
            {
                return NotFound();
            }

            // Validate foreign keys exist
            if (labResult.PatientId > 0 && !await _context.Patients.AnyAsync(p => p.Id == labResult.PatientId))
            {
                ModelState.AddModelError(nameof(LabResult.PatientId), "Selected patient does not exist.");
            }

            if (labResult.MedecinId.HasValue && labResult.MedecinId.Value > 0 && !await _context.Medecins.AnyAsync(m => m.Id == labResult.MedecinId.Value))
            {
                ModelState.AddModelError(nameof(LabResult.MedecinId), "Selected provider does not exist.");
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Lab result {LabResultId} edit validation failed", id);
                await PopulateLookupsAsync();
                return View(labResult);
            }

            try
            {
                _context.Update(labResult);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Lab result {LabResultId} updated successfully", id);
                TempData["StatusMessage"] = "Lab result updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LabResultExists(labResult.Id))
                {
                    _logger.LogWarning("Lab result {LabResultId} not found during edit", id);
                    return NotFound();
                }

                _logger.LogError("Concurrency exception while editing lab result {LabResultId}", id);
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

            var labResult = await _context.LabResults
                .Include(l => l.Patient)
                .Include(l => l.Medecin)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (labResult == null)
            {
                return NotFound();
            }

            return View(labResult);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var labResult = await _context.LabResults.FindAsync(id);
            if (labResult == null)
            {
                _logger.LogWarning("Attempted to delete non-existent lab result with ID {LabResultId}", id);
                return NotFound();
            }

            try
            {
                _context.LabResults.Remove(labResult);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Lab result {LabResultId} deleted successfully", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting lab result {LabResultId}", id);
                TempData["StatusMessage"] = "Error: Failed to delete lab result. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Export()
        {
            var results = await _context.LabResults
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .OrderByDescending(r => r.CollectedOn)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Id,Patient,Test,Priority,Status,CollectedOn,Provider");

            foreach (var r in results)
            {
                var patient = r.Patient == null ? "Unknown" : $"{r.Patient.Prenom} {r.Patient.Nom}".Trim();
                var provider = r.Medecin?.NomPrenom ?? "Unassigned";
                builder.AppendLine($"{r.Id},\"{patient}\",\"{r.TestName}\",\"{r.Priority}\",\"{r.Status}\",{r.CollectedOn:O},\"{provider}\"");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "lab-results.csv");
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

        private Task<bool> LabResultExists(int id)
        {
            return _context.LabResults.AnyAsync(e => e.Id == id);
        }
    }
}
