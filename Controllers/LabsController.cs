using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text;
using MedicalOfficeManagement.Models;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.ClinicalTeam)]
    public class LabsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public LabsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var results = await _context.LabResults
                .AsNoTracking()
                .Include(l => l.Patient)
                .Include(l => l.Medecin)
                .OrderByDescending(l => l.CollectedOn)
                .ToListAsync();

            var mapped = results.Select(r => new LabResultViewModel
            {
                Id = r.Id,
                TestName = r.TestName,
                PatientName = r.Patient == null ? "Unknown patient" : $"{r.Patient.Prenom} {r.Patient.Nom}".Trim(),
                Priority = r.Priority,
                Status = r.Status,
                CollectedOn = r.CollectedOn
            }).ToList();

            var viewModel = new LabIndexViewModel
            {
                PendingResults = mapped.Count(r => string.Equals(r.Status, "Pending", StringComparison.OrdinalIgnoreCase) || string.Equals(r.Status, "Pending Review", StringComparison.OrdinalIgnoreCase)),
                CriticalFindings = mapped.Count(r => string.Equals(r.Priority, "STAT", StringComparison.OrdinalIgnoreCase)),
                CompletedToday = mapped.Count(r => r.CollectedOn.Date == DateTime.UtcNow.Date && string.Equals(r.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                Results = mapped
            };

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
                await PopulateLookupsAsync();
                return View(labResult);
            }

            _context.LabResults.Add(labResult);
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
                await PopulateLookupsAsync();
                return View(labResult);
            }

            try
            {
                _context.Update(labResult);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await LabResultExists(labResult.Id))
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
            if (labResult != null)
            {
                _context.LabResults.Remove(labResult);
                await _context.SaveChangesAsync();
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
