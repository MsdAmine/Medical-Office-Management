// File: Controllers/AppointmentsController.cs
using System;
using System.Collections.Generic;
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
                .Include(r => r.Salle)
                .Where(r => r.DateDebut.Date >= DateTime.Today.AddDays(-7))
                .OrderBy(r => r.DateDebut)
                .ToListAsync();

            var allVisits = await _context.RendezVous
                .Include(r => r.Medecin)
                .ToListAsync();

            var patientVisits = allVisits
                .GroupBy(r => r.PatientId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Last = g.OrderByDescending(x => x.DateFin).FirstOrDefault(),
                        Next = g.Where(x => x.DateDebut > DateTime.Now).OrderBy(x => x.DateDebut).FirstOrDefault()
                    });

            var overlappingIds = new HashSet<int>();
            for (var i = 0; i < appointments.Count; i++)
            {
                for (var j = i + 1; j < appointments.Count; j++)
                {
                    var a = appointments[i];
                    var b = appointments[j];
                    if (a.DateDebut < b.DateFin && a.DateFin > b.DateDebut)
                    {
                        overlappingIds.Add(a.Id);
                        overlappingIds.Add(b.Id);
                    }
                }
            }

            var appointmentViewModels = appointments
                .Select(r =>
                {
                    var status = NormalizeStatus(r.Statut);
                    var durationMinutes = Math.Max(15, (int)(r.DateFin - r.DateDebut).TotalMinutes);
                    patientVisits.TryGetValue(r.PatientId, out var visitInfo);
                    var lastVisit = visitInfo?.Last?.DateFin;
                    var nextVisit = visitInfo?.Next?.DateDebut;
                    var riskFlags = new List<string>();
                    if (r.Patient.Antecedents?.Contains("risk", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        riskFlags.Add("High Risk");
                    }
                    if (lastVisit == null || lastVisit.Value < DateTime.Now.AddDays(-60))
                    {
                        riskFlags.Add("Follow-up Due");
                    }

                    return new AppointmentViewModel
                    {
                        Id = r.Id,
                        Time = r.DateDebut,
                        EndTime = r.DateFin,
                        PatientName = $"{r.Patient.Prenom} {r.Patient.Nom}",
                        DoctorName = r.Medecin.NomPrenom,
                        Status = status,
                        StatusColor = MapStatusColor(status),
                        StatusIcon = MapStatusIcon(status),
                        VisitType = string.IsNullOrWhiteSpace(r.Motif) ? "Consultation" : r.Motif,
                        DurationLabel = $"{durationMinutes}m",
                        Room = r.Salle?.Nom ?? "Unassigned",
                        IsLate = status is not "Cancelled" and not "Completed" && r.DateDebut < DateTime.Now,
                        HasOverlap = overlappingIds.Contains(r.Id),
                        PatientLastVisitRelative = FormatRelative(lastVisit),
                        PatientUpcoming = nextVisit?.ToString("MMM dd, h:mm tt") ?? "None scheduled",
                        PatientRiskFlags = riskFlags.Any() ? riskFlags : new List<string> { "Chronic" }
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

        private static string MapStatusIcon(string status)
        {
            return status switch
            {
                "Confirmed" => "check",
                "Waiting" => "clock",
                "Cancelled" => "x",
                "Completed" => "check-circle",
                _ => "clock"
            };
        }

        private static string FormatRelative(DateTime? dateTime)
        {
            if (dateTime == null) return "No visits yet";

            var diff = DateTime.Now - dateTime.Value;
            if (diff.TotalDays < 1) return "Today";
            if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
            if (diff.TotalDays < 30) return $"{(int)(diff.TotalDays / 7)} weeks ago";
            return $"{(int)(diff.TotalDays / 30)} months ago";
        }
    }
}
