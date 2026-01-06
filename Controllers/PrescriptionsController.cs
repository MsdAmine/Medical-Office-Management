using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Models;


namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.AdminOrMedecin)]
    public class PrescriptionsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public PrescriptionsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var prescriptions = await _context.Prescriptions
                .AsNoTracking()
                .Include(p => p.Patient)
                .Include(p => p.Medecin)
                .OrderByDescending(p => p.IssuedOn)
                .ToListAsync();

            var mapped = prescriptions.Select(p => new PrescriptionViewModel
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

            var viewModel = new PrescriptionIndexViewModel
            {
                ActiveCount = mapped.Count(p => string.Equals(p.Status, "Active", StringComparison.OrdinalIgnoreCase)),
                PendingCount = mapped.Count(p => string.Equals(p.Status, "Pending", StringComparison.OrdinalIgnoreCase)),
                CompletedCount = mapped.Count(p => string.Equals(p.Status, "Completed", StringComparison.OrdinalIgnoreCase)),
                Prescriptions = mapped
            };

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
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return View(prescription);
            }

            _context.Prescriptions.Add(prescription);
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
            if (id != prescription.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync();
                return View(prescription);
            }

            try
            {
                _context.Update(prescription);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await PrescriptionExists(prescription.Id))
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
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
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
