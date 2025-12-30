using System.Diagnostics;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MedicalOfficeManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using MedicalOfficeManagement.ViewModels.Dashboard;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(MedicalOfficeContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;

            // Récupération des compteurs réels (seront à 0 si la base est vide)
            var nbMedecins = await _context.Medecins.CountAsync();
            var nbPatients = await _context.Patients.CountAsync();
            var nbRendezVous = await _context.RendezVous.CountAsync(r => r.DateDebut.Date == today);

            // Initialisation du modèle avec vos propriétés : Label, Description, Url
            var model = new DashboardViewModel
            {
                UserDisplayName = user?.FirstName ?? "Amine",
                Now = DateTime.Now,

                // Tuiles de statistiques
                Stats = new List<StatCardViewModel>
                {
                    new StatCardViewModel { Label = "APPOINTMENTS", Value = nbRendezVous.ToString(), ColorClass = "text-brand-700" },
                    new StatCardViewModel { Label = "TOTAL PATIENTS", Value = nbPatients.ToString(), ColorClass = "text-success" },
                    new StatCardViewModel { Label = "ACTIVE DOCTORS", Value = nbMedecins.ToString(), ColorClass = "text-slate-900" }
                },

                // Actions rapides (Quick Actions)
                QuickActions = new List<QuickActionViewModel>
                {
                    new QuickActionViewModel {
                        Label = "Add Doctor",
                        Description = "Register a new provider",
                        Status = "New",
                        // Utilise "Medecin" car c'est le nom de votre dossier dans Views
                        Url = "/Medecin/Create",
                        ColorClass = "bg-brand-50 text-brand-700"
                    },
                    new QuickActionViewModel {
                        Label = "Schedule",
                        Description = "Plan visits",
                        Status = "Today",
                        Icon = "calendar",
                        // Changé de /RendezVous/ à /Appointments/ pour correspondre à image_3bf650.png
                        Url = "/Appointments/Create",
                        ColorClass = "bg-green-50 text-green-700"
                    },
                    new QuickActionViewModel {
                        Label = "Patients",
                        Description = "View records",
                        Status = "Files",
                        Icon = "users",
                        Url = "/Patients/Index",
                        ColorClass = "bg-blue-50 text-blue-700"
                    }
                }
            };

            // Envoi du modèle à la vue pour éviter le crash NullReference
            return View(model);
        }
    }
}