// File: Controllers/DoctorsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels.Doctors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    public class DoctorsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public DoctorsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var today = DateTime.Today;
            var doctors = await _context.Medecins.ToListAsync();

            var todayAppointments = await _context.RendezVous
                .Where(r => r.DateDebut.Date == today)
                .GroupBy(r => r.MedecinId)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());

            var doctorViewModels = doctors.Select(d =>
            {
                todayAppointments.TryGetValue(d.Id, out var doctorAppointments);
                var appointmentList = doctorAppointments ?? new List<RendezVou>();
                var isAvailable = !appointmentList.Any(r => r.DateDebut <= DateTime.Now && r.DateFin >= DateTime.Now);

                return new DoctorViewModel
                {
                    Id = d.Id,
                    FullName = d.NomPrenom,
                    Specialty = d.Specialite,
                    Phone = d.Telephone,
                    Email = d.Email,
                    IsAvailable = isAvailable,
                    PatientsToday = appointmentList.Count,
                    ColorClass = isAvailable ? "bg-green-50 border-green-200" : "bg-gray-50 border-gray-200"
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
