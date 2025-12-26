using System.Diagnostics;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MedicalOfficeManagement.Data; 
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize] // Nécessite une connexion pour tout le contrôleur
    public class HomeController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(MedicalOfficeContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard pour l'Administrateur
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            ViewBag.NbMedecins = await _context.Medecins.CountAsync();
            ViewBag.NbPatients = await _context.Patients.CountAsync();
            // Utilisation du nom correct de la table : RendezVous
            ViewBag.NbRendezVous = await _context.RendezVous.CountAsync();
            
            var user = await _userManager.GetUserAsync(User);
            ViewBag.FullName = user?.FirstName ?? "Admin";

            return View();
        }

        // Dashboard pour le Médecin
        [Authorize(Roles = "Medecin")]
        public async Task<IActionResult> DoctorDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Recherche du profil médecin lié à l'utilisateur mohamed
            var medecin = await _context.Medecins
                .FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

            if (medecin != null)
            {
                // Statistiques personnelles (Correction du nom de table)
                ViewBag.NbRendezVous = await _context.RendezVous
                    .Where(r => r.MedecinId == medecin.Id)
                    .CountAsync();
                
                ViewBag.DoctorName = user.FirstName;
            }

            return View();
        }
    }
}