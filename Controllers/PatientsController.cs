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
            if (!ModelState.IsValid)
                return View(patient);

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

            if (!ModelState.IsValid)
                return View(patient);

            try
            {
                _context.Update(patient);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PatientExists(patient.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(p => p.Id == id);
        }
    }
}
