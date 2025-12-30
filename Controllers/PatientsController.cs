using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.Data;
using MedicalOfficeManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace MedicalOfficeManagement.Controllers
{
    // Autorisation pour l'administrateur et le rôle de Fatima (Personnel)
    [Authorize(Roles = "Admin,Personnel")]
    public class PatientsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public PatientsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: Patients
        // Affiche la liste complète des patients enregistrés
        public async Task<IActionResult> Index()
        {
            var patients = await _context.Patients.ToListAsync();
            return View(patients);
        }

        // GET: Patients/Details/5
        // Affiche la fiche complète d'un patient (View File)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (patient == null) return NotFound();

            return View(patient);
        }

        // GET: Patients/Create
        // Affiche le formulaire d'enregistrement appelé par le Dashboard
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nom,Prenom,DateNaissance,Sexe,Telephone,Email,Adresse,Antecedents")] Patient patient)
        {
            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();
            
            return View(patient);
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nom,Prenom,DateNaissance,Sexe,Adresse,Telephone,Email,Antecedents")] Patient patient)
        {
            if (id != patient.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patient);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var patient = await _context.Patients
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null) return NotFound();

            return View(patient);
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(int id)
        {
            return _context.Patients.Any(e => e.Id == id);
        }
    }
}