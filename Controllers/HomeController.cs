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

        // Changement crucial : Autoriser "Personnel" au lieu de "Secretary"
        [Authorize(Roles = "Admin,Personnel")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;

            // Récupération des statistiques réelles
            var doctorCount = await _context.Medecins.CountAsync();
            var patientCount = await _context.Patients.CountAsync();
            var todayAppts = await _context.RendezVous.CountAsync(r => r.DateDebut.Date == today);
            var unreadMessages = Math.Max(0, (int)Math.Round(todayAppts * 0.35));
            var clinicStatusLabel = todayAppts > 12 ? "Running behind" : "On schedule";
            var clinicStatusTone = todayAppts > 12 ? "warning" : "success";

            var model = new DashboardViewModel
            {
                // Affiche le nom de Fatima ou Amine selon la session
                UserDisplayName = user?.FirstName ?? "Utilisateur",
                Now = DateTime.Now,
                Stats = new List<StatCardViewModel>(),
                QuickActions = new List<QuickActionViewModel>(),
                UnreadMessages = unreadMessages,
                ClinicStatusLabel = clinicStatusLabel,
                ClinicStatusTone = clinicStatusTone
            };

            // --- LOGIQUE ADMIN ---
            if (User.IsInRole("Admin"))
            {
                model.Stats.Add(new StatCardViewModel
                {
                    Label = "ACTIVE DOCTORS",
                    Value = doctorCount.ToString(),
                    ColorClass = "text-slate-900",
                    Icon = "users",
                    Subtext = "+2 vs yesterday",
                    TrendTone = "positive"
                });
                model.Stats.Add(new StatCardViewModel
                {
                    Label = "TODAY'S APPOINTMENTS",
                    Value = todayAppts.ToString(),
                    ColorClass = "text-brand-700",
                    Icon = "calendar",
                    Subtext = "-5% this week",
                    TrendTone = "negative"
                });

                model.QuickActions.Add(new QuickActionViewModel { 
                    Label = "Add Doctor", 
                    Description = "Register a new provider", 
                    Status = "New",
                    Icon = "plus",
                    Url = "/Medecin/Create", // Dossier au singulier dans Views
                    ColorClass = "bg-brand-50 text-brand-700"
                });
            }
            // --- LOGIQUE PERSONNEL (FATIMA) ---
            else if (User.IsInRole("Personnel")) 
            {
                model.Stats.Add(new StatCardViewModel
                {
                    Label = "TODAY'S APPOINTMENTS",
                    Value = todayAppts.ToString(),
                    ColorClass = "text-brand-700",
                    Icon = "calendar",
                    Subtext = "-5% this week",
                    TrendTone = "negative"
                });
                model.Stats.Add(new StatCardViewModel
                {
                    Label = "TOTAL PATIENTS",
                    Value = patientCount.ToString(),
                    ColorClass = "text-success",
                    Icon = "users",
                    Subtext = "+8 vs last month",
                    TrendTone = "positive"
                });

                model.QuickActions.Add(new QuickActionViewModel { 
                    Label = "Register Patient", 
                    Description = "Create medical file", 
                    Status = "Start",
                    Icon = "user-plus",
                    Url = "/Patients/Create", // Dossier existant
                    ColorClass = "bg-blue-50 text-blue-700"
                });
            }

            model.Stats.Add(new StatCardViewModel
            {
                Label = "MESSAGES",
                Value = model.UnreadMessages.ToString(),
                ColorClass = model.UnreadMessages == 0 ? "text-slate-500" : "text-warning",
                Icon = "mail",
                Subtext = model.UnreadMessages > 0 ? "Unread messages" : "All caught up",
                TrendTone = model.UnreadMessages > 0 ? "neutral" : "positive",
                IsEmptyState = model.UnreadMessages == 0
            });

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
