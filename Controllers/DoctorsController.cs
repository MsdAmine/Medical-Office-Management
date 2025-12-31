// File: Controllers/DoctorsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
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

        public DoctorsController(IDoctorRepository doctorRepository, IAppointmentRepository appointmentRepository)
        {
            _doctorRepository = doctorRepository;
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ActionResult> Index()
        {
            var today = DateTime.Today;
            var cancellationToken = HttpContext.RequestAborted;
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

            var model = new DoctorsIndexViewModel
            {
                Doctors = doctorViewModels,
                TotalDoctors = doctorViewModels.Count,
                OnDutyToday = doctorViewModels.Count(d => d.PatientsToday > 0 || d.IsAvailable)
            };

            return View(model);
        }

        public ActionResult Details(int id)
        {
            return View();
        }
    }
}
