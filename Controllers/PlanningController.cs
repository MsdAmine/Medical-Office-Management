using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Data;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using MedicalOfficeManagement.Services.RealTime;
using System;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = "Admin,Secretaire")]
    public class PlanningController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly IClinicEventPublisher _eventPublisher;

        public PlanningController(MedicalOfficeContext context, IClinicEventPublisher eventPublisher)
        {
            _context = context;
            _eventPublisher = eventPublisher;
        }

        // Affiche tous les plannings
        public async Task<IActionResult> Index()
        {
            var plannings = await _context.Plannings
                .Include(p => p.Medecin)
                .OrderByDescending(p => p.DateDebut)
                .ToListAsync();
            return View(plannings);
        }

        // Formulaire de création
        public IActionResult Create()
        {
            ViewData["MedecinId"] = new SelectList(_context.Medecins, "Id", "NomPrenom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedecinId,DateDebut,DateFin,StatutDisponibilite")] Planning planning)
        {
            // On retire la validation de l'objet de navigation pour éviter les erreurs de ModelState
            ModelState.Remove("Medecin");

            if (ModelState.IsValid)
            {
                if (planning.DateFin <= planning.DateDebut)
                {
                    ModelState.AddModelError("", "La date de fin doit être postérieure à la date de début.");
                }
                else
                {
                    _context.Add(planning);
                    await _context.SaveChangesAsync();

                    var doctor = await _context.Medecins.FindAsync(planning.MedecinId);
                    await _eventPublisher.PublishDoctorAvailabilityChangedAsync(new DoctorAvailabilityUpdateDto
                    {
                        EntityId = planning.Id.ToString(),
                        EntityType = "Planning",
                        EventType = "DoctorAvailabilityChanged",
                        DoctorId = planning.MedecinId.ToString(),
                        DoctorName = doctor?.NomPrenom ?? $"Doctor {planning.MedecinId}",
                        IsAvailable = !string.Equals(planning.StatutDisponibilite, "Indisponible", StringComparison.OrdinalIgnoreCase),
                        PatientsToday = 0,
                        Date = planning.DateDebut.Date,
                        ChangeReason = "PlanningCreated",
                        AffectedViews = new[] { "Doctors", "Dashboard" }
                    }, HttpContext.RequestAborted);
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["MedecinId"] = new SelectList(_context.Medecins, "Id", "NomPrenom", planning.MedecinId);
            return View(planning);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var planning = await _context.Plannings.FindAsync(id);
            if (planning != null)
            {
                _context.Plannings.Remove(planning);
                await _context.SaveChangesAsync();

                var doctor = await _context.Medecins.FindAsync(planning.MedecinId);
                await _eventPublisher.PublishDoctorAvailabilityChangedAsync(new DoctorAvailabilityUpdateDto
                {
                    EntityId = planning.Id.ToString(),
                    EntityType = "Planning",
                    EventType = "DoctorAvailabilityChanged",
                    DoctorId = planning.MedecinId.ToString(),
                    DoctorName = doctor?.NomPrenom ?? $"Doctor {planning.MedecinId}",
                    IsAvailable = true,
                    PatientsToday = 0,
                    Date = planning.DateDebut.Date,
                    ChangeReason = "PlanningDeleted",
                    AffectedViews = new[] { "Doctors", "Dashboard" }
                }, HttpContext.RequestAborted);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
