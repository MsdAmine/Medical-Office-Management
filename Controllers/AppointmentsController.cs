// File: Controllers/AppointmentsController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels.Appointments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public AppointmentsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var appointments = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .Where(r => r.DateDebut.Date >= DateTime.Today.AddDays(-7))
                .OrderBy(r => r.DateDebut)
                .ToListAsync();

            var appointmentViewModels = appointments
                .Select(r =>
                {
                    var status = NormalizeStatus(r.Statut);
                    return new AppointmentViewModel
                    {
                        Id = r.Id,
                        Time = r.DateDebut,
                        PatientName = $"{r.Patient.Prenom} {r.Patient.Nom}",
                        DoctorName = r.Medecin.NomPrenom,
                        Status = status,
                        StatusColor = MapStatusColor(status)
                    };
                })
                .ToList();

            var model = new AppointmentsIndexViewModel
            {
                Appointments = appointmentViewModels,
                TodayCount = appointmentViewModels.Count(a => a.Time.Date == DateTime.Today),
                ConfirmedCount = appointmentViewModels.Count(a => a.Status == "Confirmed"),
                WaitingCount = appointmentViewModels.Count(a => a.Status == "Waiting")
            };

            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }

        private static string NormalizeStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return "Waiting";
            }

            return status.ToLower() switch
            {
                "confirme" or "confirmed" => "Confirmed",
                "annule" or "cancelled" or "canceled" => "Cancelled",
                "termine" or "completed" => "Completed",
                _ => "Waiting"
            };
        }

        private static string MapStatusColor(string status)
        {
            return status switch
            {
                "Confirmed" => "bg-green-100 text-green-700 border-green-200",
                "Waiting" => "bg-blue-100 text-blue-700 border-blue-200",
                "Completed" => "bg-gray-100 text-gray-700 border-gray-200",
                "Cancelled" => "bg-red-100 text-red-700 border-red-200",
                _ => "bg-gray-100 text-gray-700 border-gray-200"
            };
        }
    }
}
