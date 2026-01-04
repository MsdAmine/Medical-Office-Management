using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize] // all patient operations require login
    public class PatientsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public PatientsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: /Patients
        [Authorize(Roles = SystemRoles.AdminOrSecretaire)]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Patients";
            ViewData["Breadcrumb"] = "Patients";

            var patients = await _context.Patients
                .OrderBy(p => p.Nom)
                .ToListAsync();

            return View(patients);
        }

        // GET: /Patients/Details/5
        [Authorize(Roles = SystemRoles.AdminOrSecretaire)]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            ViewData["Title"] = "Patient Details";
            ViewData["Breadcrumb"] = "Patients";

            return View(patient);
        }

        // GET: /Patients/Create
        [Authorize(Roles = SystemRoles.Secretaire)]
        public IActionResult Create()
        {
            ViewData["Title"] = "New Patient";
            ViewData["Breadcrumb"] = "Patients";

            return View();
        }

        // POST: /Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SystemRoles.Secretaire)]
        public async Task<IActionResult> Create(Patient patient)
        {
            NormalizeContactFields(patient);
            ValidateNormalizedContact(patient);
            await ValidateContactUniquenessAsync(patient);

            if (!ModelState.IsValid)
                return View(patient);

            var now = DateTime.UtcNow;
            var userName = User?.Identity?.Name ?? "system";

            patient.CreatedAt = now;
            patient.UpdatedAt = now;
            patient.CreatedBy = userName;
            patient.UpdatedBy = userName;
            patient.IsDeleted = false;

            _context.Add(patient);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Patients/Edit/5
        [Authorize(Roles = SystemRoles.Secretaire)]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null)
                return NotFound();

            ViewData["Title"] = "Edit Patient";
            ViewData["Breadcrumb"] = "Patients";

            return View(patient);
        }

        // POST: /Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SystemRoles.Secretaire)]
        public async Task<IActionResult> Edit(int id, Patient patient)
        {
            if (id != patient.Id)
                return NotFound();

            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingPatient == null)
                return NotFound();

            NormalizeContactFields(patient);
            ValidateNormalizedContact(patient);
            await ValidateContactUniquenessAsync(patient);

            if (!ModelState.IsValid)
            {
                patient.CreatedAt = existingPatient.CreatedAt;
                patient.CreatedBy = existingPatient.CreatedBy;
                patient.UpdatedAt = existingPatient.UpdatedAt;
                patient.UpdatedBy = existingPatient.UpdatedBy;
                patient.DeletedAt = existingPatient.DeletedAt;
                patient.DeletedBy = existingPatient.DeletedBy;
                patient.IsDeleted = existingPatient.IsDeleted;

                return View(patient);
            }

            existingPatient.Nom = patient.Nom;
            existingPatient.Prenom = patient.Prenom;
            existingPatient.DateNaissance = patient.DateNaissance;
            existingPatient.Sexe = patient.Sexe;
            existingPatient.Adresse = patient.Adresse;
            existingPatient.Telephone = patient.Telephone;
            existingPatient.Email = patient.Email;
            existingPatient.Antecedents = patient.Antecedents;
            existingPatient.UpdatedAt = DateTime.UtcNow;
            existingPatient.UpdatedBy = User?.Identity?.Name ?? "system";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: /Patients/Delete/5
        [Authorize(Roles = SystemRoles.Secretaire)]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var patient = await _context.Patients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null || patient.IsDeleted)
                return NotFound();

            ViewData["Title"] = "Delete Patient";
            ViewData["Breadcrumb"] = "Patients";

            return View(patient);
        }

        // POST: /Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = SystemRoles.Secretaire)]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound();

            if (!patient.IsDeleted)
            {
                var now = DateTime.UtcNow;
                var userName = User?.Identity?.Name ?? "system";

                patient.IsDeleted = true;
                patient.DeletedAt = now;
                patient.DeletedBy = userName;
                patient.UpdatedAt = now;
                patient.UpdatedBy = userName;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(p => p.Id == id);
        }

        private void NormalizeContactFields(Patient patient)
        {
            patient.Email = NormalizeEmail(patient.Email);
            patient.Telephone = NormalizePhone(patient.Telephone);
        }

        private string? NormalizeEmail(string? email)
        {
            return string.IsNullOrWhiteSpace(email)
                ? null
                : email.Trim().ToLowerInvariant();
        }

        private string NormalizePhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return string.Empty;

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.Length == 0)
                return string.Empty;

            return $"+{digits}";
        }

        private void ValidateNormalizedContact(Patient patient)
        {
            var phoneDigitCount = patient.Telephone?.Count(char.IsDigit) ?? 0;

            if (string.IsNullOrWhiteSpace(patient.Telephone))
            {
                ModelState.AddModelError(nameof(Patient.Telephone), "Phone number is required.");
            }
            else if (phoneDigitCount < 8)
            {
                ModelState.AddModelError(nameof(Patient.Telephone), "Phone number must contain at least 8 digits.");
            }

            if (string.IsNullOrWhiteSpace(patient.Email))
            {
                ModelState.AddModelError(nameof(Patient.Email), "Email is required.");
            }
        }

        private async Task ValidateContactUniquenessAsync(Patient patient)
        {
            if (!string.IsNullOrWhiteSpace(patient.Email))
            {
                var emailExists = await _context.Patients
                    .AnyAsync(p => p.Email == patient.Email && p.Id != patient.Id);

                if (emailExists)
                {
                    ModelState.AddModelError(nameof(Patient.Email), "A patient with this email already exists.");
                }
            }

            if (!string.IsNullOrWhiteSpace(patient.Telephone))
            {
                var phoneExists = await _context.Patients
                    .AnyAsync(p => p.Telephone == patient.Telephone && p.Id != patient.Id);

                if (phoneExists)
                {
                    ModelState.AddModelError(nameof(Patient.Telephone), "A patient with this phone number already exists.");
                }
            }
        }
    }
}
