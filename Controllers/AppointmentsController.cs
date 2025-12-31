// File: Controllers/AppointmentsController.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedicalOfficeManagement.Data.Entities;
using MedicalOfficeManagement.Data.Repositories;
using MedicalOfficeManagement.ViewModels.Appointments;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentsController(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ActionResult> Index()
        {
            var cancellationToken = HttpContext.RequestAborted;
            var windowStart = DateTime.Today.AddDays(-7);
            var windowEnd = DateTime.Today.AddDays(14);
            var appointments = await _appointmentRepository.ListRangeAsync(windowStart, windowEnd, cancellationToken);

            var allVisits = await _appointmentRepository.ListAsync(cancellationToken);

            var patientVisits = allVisits
                .GroupBy(r => r.PatientId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Last = g.OrderByDescending(x => x.EndTime).FirstOrDefault(),
                        Next = g.Where(x => x.StartTime > DateTime.Now).OrderBy(x => x.StartTime).FirstOrDefault()
                    });

            var overlappingIds = new HashSet<int>();
            for (var i = 0; i < appointments.Count; i++)
            {
                for (var j = i + 1; j < appointments.Count; j++)
                {
                    var a = appointments[i];
                    var b = appointments[j];
                    if (a.StartTime < b.EndTime && a.EndTime > b.StartTime)
                    {
                        overlappingIds.Add(Math.Abs(BitConverter.ToInt32(a.Id.ToByteArray(), 0)));
                        overlappingIds.Add(Math.Abs(BitConverter.ToInt32(b.Id.ToByteArray(), 0)));
                    }
                }
            }

            var appointmentViewModels = appointments
                .Select(r =>
                {
                    var status = MapStatus(r.Status);
                    var durationMinutes = Math.Max(15, (int)(r.EndTime - r.StartTime).TotalMinutes);
                    patientVisits.TryGetValue(r.PatientId, out var visitInfo);
                    var lastVisit = visitInfo?.Last?.EndTime;
                    var nextVisit = visitInfo?.Next?.StartTime;
                    var riskFlags = new List<string>();
                    if (string.Equals(r.Patient?.RiskLevel, "High", StringComparison.OrdinalIgnoreCase))
                    {
                        riskFlags.Add("High Risk");
                    }
                    if (lastVisit == null || lastVisit.Value < DateTime.Now.AddDays(-60))
                    {
                        riskFlags.Add("Follow-up Due");
                    }

                    return new AppointmentViewModel
                    {
                        Id = Math.Abs(BitConverter.ToInt32(r.Id.ToByteArray(), 0)),
                        Time = r.StartTime,
                        EndTime = r.EndTime,
                        PatientName = $"{r.Patient?.FirstName} {r.Patient?.LastName}".Trim(),
                        DoctorName = r.Doctor?.FullName ?? "Unassigned",
                        Status = status,
                        StatusColor = MapStatusColor(status),
                        StatusIcon = MapStatusIcon(status),
                        VisitType = "Consultation",
                        DurationLabel = $"{durationMinutes}m",
                        Room = r.Room ?? "Unassigned",
                        IsLate = status is not "Cancelled" and not "Completed" && r.StartTime < DateTime.Now,
                        HasOverlap = overlappingIds.Contains(Math.Abs(BitConverter.ToInt32(r.Id.ToByteArray(), 0))),
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

        private static string MapStatus(AppointmentStatus status) =>
            status switch
            {
                AppointmentStatus.Confirmed => "Confirmed",
                AppointmentStatus.Completed => "Completed",
                AppointmentStatus.Cancelled => "Cancelled",
                _ => "Waiting"
            };

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
