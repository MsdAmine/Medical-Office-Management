using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.Patients;
using Microsoft.AspNetCore.Authorization;
using MedicalOfficeManagement.Data.Repositories;

namespace MedicalOfficeManagement.Controllers
{
    // Autorisation pour l'administrateur et le rôle de Fatima (Personnel)
    [Authorize(Roles = "Admin,Personnel")]
    public class PatientsController : Controller
    {
        private readonly IPatientRepository _patientRepository;

        public PatientsController(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }

        // GET: Patients
        // Affiche la liste complète des patients enregistrés
        public async Task<IActionResult> Index()
        {
            var cancellationToken = HttpContext.RequestAborted;
            var patients = await _patientRepository.ListWithAppointmentsAsync(cancellationToken);

            var patientViewModels = patients.Select(p =>
            {
                var lastVisit = p.Appointments
                    .OrderByDescending(r => r.EndTime)
                    .FirstOrDefault();

                var flags = new List<string>();
                if (!string.IsNullOrWhiteSpace(p.RiskLevel))
                {
                    flags.Add(p.RiskLevel);
                }

                if (lastVisit == null || lastVisit.EndTime < DateTime.Now.AddDays(-60))
                {
                    flags.Add("Follow-up Due");
                }

                if (string.Equals(p.RiskLevel, "High", StringComparison.OrdinalIgnoreCase))
                {
                    flags.Add("High Risk");
                }

                return new PatientViewModel
                {
                    Id = Math.Abs(BitConverter.ToInt32(p.Id.ToByteArray(), 0)),
                    FullName = $"{p.FirstName} {p.LastName}",
                    Phone = p.Phone ?? "N/A",
                    Email = p.Email ?? "N/A",
                    LastVisit = lastVisit?.EndTime,
                    LastVisitRelative = lastVisit?.EndTime switch
                    {
                        null => "No visits yet",
                        DateTime dt when (DateTime.Now - dt).TotalDays < 1 => "Today",
                        DateTime dt when (DateTime.Now - dt).TotalDays < 7 => $"{(int)(DateTime.Now - dt).TotalDays} days ago",
                        DateTime dt when (DateTime.Now - dt).TotalDays < 30 => $"{(int)((DateTime.Now - dt).TotalDays / 7)} weeks ago",
                        _ => $"{(int)((DateTime.Now - lastVisit!.EndTime).TotalDays / 30)} months ago"
                    },
                    PrimaryDoctor = lastVisit?.Doctor?.FullName ?? "Unassigned",
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
            return NotFound();
        }

        // GET: Patients/Create
        // Affiche le formulaire d'enregistrement appelé par le Dashboard
        public IActionResult Create()
        {
            return RedirectToAction(nameof(Index));
        }

        // POST: Patients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(object _)
        {
            return RedirectToAction(nameof(Index));
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // POST: Patients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, object _)
        {
            return RedirectToAction(nameof(Index));
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            return RedirectToAction(nameof(Index));
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            return RedirectToAction(nameof(Index));
        }
    }
}
