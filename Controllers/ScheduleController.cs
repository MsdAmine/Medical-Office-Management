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

        // Affiche l'emploi du temps hebdomadaire
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

        public IActionResult Create()
        {
            ViewData["MedecinId"] = new SelectList(_context.Medecins, "Id", "NomPrenom");
            ViewData["SalleId"] = new SelectList(_context.Salles, "Id", "Nom");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedecinId,SalleId,JourSemaine,HeureArrivee,HeureDepart,Note")] Schedule schedule)
        {
            ModelState.Remove("Medecin");
            ModelState.Remove("Salle");

            if (ModelState.IsValid)
            {
                _context.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MedecinId"] = new SelectList(_context.Medecins, "Id", "NomPrenom", schedule.MedecinId);
            ViewData["SalleId"] = new SelectList(_context.Salles, "Id", "Nom", schedule.SalleId);
            return View(schedule);
        }

        [HttpPost]
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