using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.Data;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels.Patients;
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
            var patients = await _context.Patients
                .Include(p => p.RendezVous)
                .ThenInclude(r => r.Medecin)
                .ToListAsync();

            var patientViewModels = patients.Select(p =>
            {
                var lastVisit = p.RendezVous
                    .OrderByDescending(r => r.DateFin)
                    .FirstOrDefault();

                var flags = new List<string>();
                if (!string.IsNullOrWhiteSpace(p.Antecedents))
                {
                    flags.Add("Chronic");
                }

                if (lastVisit == null || lastVisit.DateFin < DateTime.Now.AddDays(-60))
                {
                    flags.Add("Follow-up Due");
                }

                if (p.Antecedents?.Contains("risk", StringComparison.OrdinalIgnoreCase) == true)
                {
                    flags.Add("High Risk");
                }

                return new PatientViewModel
                {
                    Id = p.Id,
                    FullName = $"{p.Prenom} {p.Nom}",
                    Phone = p.Telephone ?? "N/A",
                    Email = p.Email ?? "N/A",
                    LastVisit = lastVisit?.DateFin,
                    LastVisitRelative = lastVisit?.DateFin switch
                    {
                        null => "No visits yet",
                        DateTime dt when (DateTime.Now - dt).TotalDays < 1 => "Today",
                        DateTime dt when (DateTime.Now - dt).TotalDays < 7 => $"{(int)(DateTime.Now - dt).TotalDays} days ago",
                        DateTime dt when (DateTime.Now - dt).TotalDays < 30 => $"{(int)((DateTime.Now - dt).TotalDays / 7)} weeks ago",
                        _ => $"{(int)((DateTime.Now - lastVisit!.DateFin).TotalDays / 30)} months ago"
                    },
                    PrimaryDoctor = lastVisit?.Medecin?.NomPrenom ?? "Unassigned",
                    ClinicalFlags = flags.Any() ? flags : new List<string> { "Chronic" }
                };
            }).ToList();

            var model = new PatientsIndexViewModel
            {
                Patients = patientViewModels,
                TotalPatients = patientViewModels.Count,
                ActivePatients = patientViewModels.Count,
                NewThisMonth = patientViewModels.Count(p => (p.LastVisit ?? DateTime.Now.AddMonths(-1)) >= DateTime.Now.AddMonths(-1))
            };

            return View(model);
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
