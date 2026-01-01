using System.Diagnostics;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MedicalOfficeManagement.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using MedicalOfficeManagement.ViewModels.Dashboard;
using MedicalOfficeManagement.Services;
using System.Threading;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPatientRepository _patientRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IWorkloadService _workloadService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IPatientRepository patientRepository,
            IAppointmentRepository appointmentRepository,
            IDoctorRepository doctorRepository,
            IWorkloadService workloadService)
        {
            _userManager = userManager;
            _patientRepository = patientRepository;
            _appointmentRepository = appointmentRepository;
            _doctorRepository = doctorRepository;
            _workloadService = workloadService;
        }

        // Changement crucial : Autoriser "Personnel" au lieu de "Secretary"
        [Authorize(Roles = "Admin,Personnel")]
        public async Task<IActionResult> Index()
        {
            var cancellationToken = HttpContext.RequestAborted;
            var model = await BuildDashboardViewModel(
                ParseBucketMinutes(Request.Query["bucketMinutes"]),
                ParseHour(Request.Query["startHour"], 8),
                ParseHour(Request.Query["endHour"], 18),
                cancellationToken);

            return View(model);
        }

        [HttpGet]
        public async Task<PartialViewResult> DashboardLiveTiles(int? bucketMinutes, int? startHour, int? endHour)
        {
            var cancellationToken = HttpContext.RequestAborted;
            var model = await BuildDashboardViewModel(
                ParseBucketMinutes(bucketMinutes?.ToString() ?? Request.Query["bucketMinutes"]),
                ParseHour(startHour?.ToString() ?? Request.Query["startHour"], 8),
                ParseHour(endHour?.ToString() ?? Request.Query["endHour"], 18),
                cancellationToken);
            return PartialView("~/Views/Dashboard/_LiveTiles.cshtml", model);
        }

        private static int ParseBucketMinutes(string? input)
        {
            if (int.TryParse(input, out var parsed) && (parsed == 15 || parsed == 30 || parsed == 60))
            {
                return parsed;
            }

            return 30;
        }

        private static int ParseHour(string? input, int fallback)
        {
            if (int.TryParse(input, out var parsed) && parsed >= 0 && parsed <= 23)
            {
                return parsed;
            }

            return fallback;
        }

        private async Task<DashboardViewModel> BuildDashboardViewModel(
            int bucketMinutes,
            int startHour,
            int endHour,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            var today = DateTime.Today;
            if (endHour <= startHour)
            {
                endHour = Math.Min(23, startHour + 1);
            }

            var doctors = await _doctorRepository.ListAsync(cancellationToken);
            var patients = await _patientRepository.ListAsync(cancellationToken);
            var todayAppts = await _appointmentRepository.ListForDateAsync(today, cancellationToken);
            var clinicHeatmap = await _workloadService.GetClinicHeatmapAsync(
                today,
                bucketMinutes,
                startHour,
                endHour,
                cancellationToken);

            var doctorCount = doctors.Count;
            var patientCount = patients.Count;
            var appointmentCount = todayAppts.Count;
            var unreadMessages = Math.Max(0, (int)Math.Round(appointmentCount * 0.35));
            var clinicStatusLabel = appointmentCount > 12 ? "Running behind" : "On schedule";
            var clinicStatusTone = appointmentCount > 12 ? "warning" : "success";

            var model = new DashboardViewModel
            {
                UserDisplayName = user?.FirstName ?? "Utilisateur",
                Now = DateTime.Now,
                Stats = new List<StatCardViewModel>(),
                QuickActions = new List<QuickActionViewModel>(),
                UnreadMessages = unreadMessages,
                ClinicStatusLabel = clinicStatusLabel,
                ClinicStatusTone = clinicStatusTone,
                ClinicHeatmap = clinicHeatmap,
                SelectedBucketMinutes = bucketMinutes,
                StartHour = startHour,
                EndHour = endHour
            };

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
                    Value = appointmentCount.ToString(),
                    ColorClass = "text-brand-700",
                    Icon = "calendar",
                    Subtext = "-5% this week",
                    TrendTone = "negative"
                });

                model.QuickActions.Add(new QuickActionViewModel
                {
                    Label = "Add Doctor",
                    Description = "Register a new provider",
                    Status = "New",
                    Icon = "plus",
                    Url = "/Medecin/Create",
                    ColorClass = "bg-brand-50 text-brand-700"
                });
            }
            else if (User.IsInRole("Personnel"))
            {
                model.Stats.Add(new StatCardViewModel
                {
                    Label = "TODAY'S APPOINTMENTS",
                    Value = appointmentCount.ToString(),
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

                model.QuickActions.Add(new QuickActionViewModel
                {
                    Label = "Register Patient",
                    Description = "Create medical file",
                    Status = "Start",
                    Icon = "user-plus",
                    Url = "/Patients/Create",
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

            return model;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
