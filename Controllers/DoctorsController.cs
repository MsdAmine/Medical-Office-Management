// File: Controllers/DoctorsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MedicalOfficeManagement.Data.Repositories;
using MedicalOfficeManagement.ViewModels.Doctors;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IWorkloadService _workloadService;

        public DoctorsController(
            IDoctorRepository doctorRepository,
            IAppointmentRepository appointmentRepository,
            IWorkloadService workloadService)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
            _workloadService = workloadService;
        }

        public async Task<ActionResult> Index()
        {
            var cancellationToken = HttpContext.RequestAborted;
            var model = await BuildDoctorsViewModel(
                ParseBucketMinutes(Request.Query["bucketMinutes"]),
                ParseHour(Request.Query["startHour"], 8),
                ParseHour(Request.Query["endHour"], 18),
                cancellationToken);
            return View(model);
        }

        [HttpGet]
        public async Task<PartialViewResult> LivePanel(int? bucketMinutes, int? startHour, int? endHour)
        {
            var cancellationToken = HttpContext.RequestAborted;
            var model = await BuildDoctorsViewModel(
                ParseBucketMinutes(bucketMinutes?.ToString() ?? Request.Query["bucketMinutes"]),
                ParseHour(startHour?.ToString() ?? Request.Query["startHour"], 8),
                ParseHour(endHour?.ToString() ?? Request.Query["endHour"], 18),
                cancellationToken);
            return PartialView("_DoctorsLiveSection", model);
        }

        public ActionResult Details(int id)
        {
            return View();
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

        private async Task<DoctorsIndexViewModel> BuildDoctorsViewModel(
            int bucketMinutes,
            int startHour,
            int endHour,
            CancellationToken cancellationToken)
        {
            var today = DateTime.Today;
            if (endHour <= startHour)
            {
                endHour = Math.Min(23, startHour + 1);
            }

            var doctors = await _doctorRepository.ListAsync(cancellationToken);

            var todayAppointments = (await _appointmentRepository.ListForDateAsync(today, cancellationToken))
                .GroupBy(r => r.DoctorId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var doctorViewModels = doctors.Select(d =>
            {
                todayAppointments.TryGetValue(d.Id, out var doctorAppointments);
                var appointmentList = doctorAppointments ?? new List<Data.Entities.AppointmentEntity>();
                var isAvailable = !appointmentList.Any(r => r.StartTime <= DateTime.Now && r.EndTime >= DateTime.Now);
                var firstAppointment = appointmentList.OrderBy(r => r.StartTime).FirstOrDefault();
                var lastAppointment = appointmentList.OrderByDescending(r => r.EndTime).FirstOrDefault();
                var nextSlot = appointmentList.Where(r => r.EndTime > DateTime.Now).OrderBy(r => r.EndTime).FirstOrDefault();

                return new DoctorViewModel
                {
                    Id = Math.Abs(d.Id.GetHashCode()),
                    FullName = d.FullName,
                    Specialty = d.Specialty,
                    Phone = d.Phone ?? "N/A",
                    Email = d.Email ?? "N/A",
                    IsAvailable = isAvailable,
                    PatientsToday = appointmentList.Count,
                    ColorClass = isAvailable ? "bg-green-50 border-green-200" : "bg-gray-50 border-gray-200",
                    Role = "MD",
                    Location = d.Location ?? "Primary Clinic",
                    Language = "English / French",
                    FirstAppointment = firstAppointment?.StartTime.ToString("h:mm tt") ?? "—",
                    LastAppointment = lastAppointment?.EndTime.ToString("h:mm tt") ?? "—",
                    NextAvailableSlot = nextSlot == null ? "Now" : nextSlot.EndTime.ToString("h:mm tt")
                };
            }).ToList();

            return new DoctorsIndexViewModel
            {
                Doctors = doctorViewModels,
                TotalDoctors = doctorViewModels.Count,
                OnDutyToday = doctorViewModels.Count(d => d.PatientsToday > 0 || d.IsAvailable),
                WorkloadHeatmap = await _workloadService.GetDoctorsHeatmapAsync(
                    today,
                    bucketMinutes,
                    startHour,
                    endHour,
                    cancellationToken),
                SelectedBucketMinutes = bucketMinutes,
                StartHour = startHour,
                EndHour = endHour
            };
        }
    }
}
