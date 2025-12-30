using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Authorization;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = "Admin,Secretaire")]
    public class ScheduleController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public ScheduleController(MedicalOfficeContext context)
        {
            _context = context;
        }

        // GET: Schedule
        // Affiche la liste des affectations actuelles
        public async Task<IActionResult> Index()
        {
            var schedules = await _context.Schedules
                .Include(s => s.Medecin)
                .Include(s => s.Salle)
                .OrderBy(s => s.JourSemaine)
                .ThenBy(s => s.HeureArrivee)
                .ToListAsync();
            return View(schedules);
        }

        // GET: Schedule/Create
        public IActionResult Create()
        {
            ViewData["MedecinId"] = new SelectList(_context.Medecins, "Id", "NomPrenom");
            ViewData["SalleId"] = new SelectList(_context.Salles, "Id", "Nom");
            return View();
        }

        // POST: Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedecinId,SalleId,JourSemaine,HeureArrivee,HeureDepart,Note")] Schedule schedule)
        {
            // On retire les propriétés de navigation pour éviter les erreurs de validation du modèle
            ModelState.Remove("Medecin");
            ModelState.Remove("Salle");

            if (ModelState.IsValid)
            {
                // LOGIQUE DE DÉTECTION DE CONFLIT
                // Vérifie si la même salle est occupée le même jour sur le même créneau
                bool hasConflict = await _context.Schedules
                    .AnyAsync(s => s.SalleId == schedule.SalleId
                                && s.JourSemaine == schedule.JourSemaine
                                && (
                                    (schedule.HeureArrivee >= s.HeureArrivee && schedule.HeureArrivee < s.HeureDepart) ||
                                    (schedule.HeureDepart > s.HeureArrivee && schedule.HeureDepart <= s.HeureDepart) ||
                                    (schedule.HeureArrivee <= s.HeureArrivee && schedule.HeureDepart >= s.HeureDepart)
                                ));

                if (hasConflict)
                {
                    // Ajoute une erreur globale affichée dans le validation-summary de la vue
                    ModelState.AddModelError(string.Empty, "Conflit d'affectation : Cette salle est déjà réservée pour ce créneau horaire.");
                }
                else
                {
                    _context.Add(schedule);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // En cas d'échec ou de conflit, on recharge les listes pour la vue
            ViewData["MedecinId"] = new SelectList(_context.Medecins, "Id", "NomPrenom", schedule.MedecinId);
            ViewData["SalleId"] = new SelectList(_context.Salles, "Id", "Nom", schedule.SalleId);
            return View(schedule);
        }

        // POST: Schedule/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule != null)
            {
                _context.Schedules.Remove(schedule);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}