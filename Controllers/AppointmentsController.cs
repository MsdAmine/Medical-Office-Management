using System;
using System.Linq;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MedicalOfficeManagement.Models.Security;
using System.Security.Claims;
using System.Text;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.SchedulingTeam)]
    public class AppointmentsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public AppointmentsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                return Forbid();
            }

            var appointments = await GetScopedAppointmentsQueryable(medecinId)
                .OrderBy(r => r.DateDebut)
                .ToListAsync();

            var appointmentViewModels = appointments
                .Select(MapToViewModel)
                .ToList();

            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var viewModel = new ScheduleIndexViewModel
            {
                UpcomingCount = appointments.Count(a => a.DateDebut.Date >= DateTime.Today),
                InClinicToday = appointments.Count(a => a.DateDebut.Date == DateTime.Today),
                CompletedThisWeek = appointments.Count(a =>
                    IsCompleted(a) &&
                    a.DateDebut >= startOfWeek &&
                    a.DateDebut < endOfWeek),
                Appointments = appointmentViewModels
            };

            SetSchedulePageMetadata("Schedule");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await BuildAppointmentQuery()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            SetSchedulePageMetadata("Appointment Details");
            return View(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                return Forbid();
            }

            var viewModel = new ScheduleFormViewModel
            {
                Appointment = new RendezVou
                {
                    DateDebut = DateTime.Today.AddHours(9),
                    DateFin = DateTime.Today.AddHours(10),
                    Statut = "Scheduled"
                },
                Patients = await GetPatientsSelectListAsync(selectedId: null, medecinId),
                Medecins = await GetMedecinsSelectListAsync(selectedId: medecinId, medecinId: medecinId)
            };

            if (medecinId.HasValue && User.IsInRole(SystemRoles.Medecin))
            {
                viewModel.Appointment.MedecinId = medecinId.Value;
            }

            SetSchedulePageMetadata("New Appointment");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleFormViewModel viewModel)
        {
            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                return Forbid();
            }

            await ValidateAppointmentAsync(viewModel.Appointment, medecinId);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel, medecinId);
                SetSchedulePageMetadata("New Appointment");
                return View(viewModel);
            }

            _context.RendezVous.Add(viewModel.Appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.RendezVous.FindAsync(id);
            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            var medecinId = await GetCurrentMedecinIdAsync();

            var viewModel = new ScheduleFormViewModel
            {
                Appointment = appointment,
                Patients = await GetPatientsSelectListAsync(appointment.PatientId, medecinId),
                Medecins = await GetMedecinsSelectListAsync(appointment.MedecinId, medecinId)
            };

            SetSchedulePageMetadata("Edit Appointment");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ScheduleFormViewModel viewModel)
        {
            if (id != viewModel.Appointment.Id)
                return NotFound();

            var existingAppointment = await _context.RendezVous
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (existingAppointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(existingAppointment))
                return Forbid();

            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin))
            {
                if (!medecinId.HasValue)
                {
                    return Forbid();
                }

                viewModel.Appointment.MedecinId = medecinId.Value;
            }

            await ValidateAppointmentAsync(viewModel.Appointment, medecinId);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel, medecinId);
                SetSchedulePageMetadata("Edit Appointment");
                return View(viewModel);
            }

            try
            {
                _context.Update(viewModel.Appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AppointmentExists(viewModel.Appointment.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await BuildAppointmentQuery()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            SetSchedulePageMetadata("Delete Appointment");
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.RendezVous.FindAsync(id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            _context.RendezVous.Remove(appointment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CalendarEvents(DateTime? start, DateTime? end)
        {
            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                return Forbid();
            }

            var query = GetScopedAppointmentsQueryable(medecinId);

            if (start.HasValue)
            {
                query = query.Where(r => r.DateFin >= start.Value);
            }

            if (end.HasValue)
            {
                query = query.Where(r => r.DateDebut <= end.Value);
            }

            var appointments = await query.AsNoTracking().ToListAsync();

            var events = appointments
                .Select(r => new
                {
                    id = r.Id,
                    title = string.IsNullOrWhiteSpace(r.Motif)
                        ? BuildPatientName(r.Patient)
                        : $"{BuildPatientName(r.Patient)} - {r.Motif}",
                    start = r.DateDebut,
                    end = r.DateFin,
                    status = r.Statut,
                    patient = BuildPatientName(r.Patient),
                    medecin = string.IsNullOrWhiteSpace(r.Medecin.NomPrenom) ? "Unassigned" : r.Medecin.NomPrenom,
                    reason = string.IsNullOrWhiteSpace(r.Motif) ? "Not specified" : r.Motif,
                    location = r.SalleId.HasValue ? $"Room {r.SalleId}" : "Unassigned"
                })
                .ToList();

            return Json(events);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule([FromBody] RescheduleRequest request)
        {
            if (request == null)
            {
                return BadRequest("Missing reschedule payload.");
            }

            var appointment = await BuildAppointmentQuery()
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            if (request.End <= request.Start)
            {
                return BadRequest("End time must be after the start time.");
            }

            appointment.DateDebut = request.Start;
            appointment.DateFin = request.End;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            if (request == null)
            {
                return BadRequest("Missing status payload.");
            }

            var appointment = await _context.RendezVous.FindAsync(request.Id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            if (!AllowedStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Unsupported status value.");
            }

            appointment.Statut = request.Status;
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> DownloadIcs(int id)
        {
            var appointment = await BuildAppointmentQuery()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            var icsContent = BuildIcsContent(appointment);
            var bytes = Encoding.UTF8.GetBytes(icsContent);
            var fileName = $"appointment-{appointment.Id}.ics";

            return File(bytes, "text/calendar", fileName);
        }

        private IQueryable<RendezVou> BuildAppointmentQuery()
        {
            return _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin);
        }

        private IQueryable<RendezVou> GetScopedAppointmentsQueryable(int? medecinId)
        {
            var query = BuildAppointmentQuery();

            if (IsAdminOrSecretaire())
            {
                return query;
            }

            if (User.IsInRole(SystemRoles.Medecin) && medecinId.HasValue)
            {
                return query.Where(r => r.MedecinId == medecinId.Value);
            }

            return query.Where(_ => false);
        }

        private async Task<List<Patient>> GetScopedPatientsAsync(int? medecinId)
        {
            if (IsAdminOrSecretaire())
            {
                return await _context.Patients
                    .OrderBy(p => p.Nom)
                    .ThenBy(p => p.Prenom)
                    .ToListAsync();
            }

            if (User.IsInRole(SystemRoles.Medecin) && medecinId.HasValue)
            {
                return await _context.Patients
                    .Where(p =>
                        _context.RendezVous.Any(r => r.PatientId == p.Id && r.MedecinId == medecinId.Value)
                        || _context.Consultations.Any(c => c.PatientId == p.Id && c.MedecinId == medecinId.Value))
                    .OrderBy(p => p.Nom)
                    .ThenBy(p => p.Prenom)
                    .ToListAsync();
            }

            return new List<Patient>();
        }

        private async Task<List<int>> GetMedecinPatientIdsAsync(int medecinId)
        {
            var idsFromAppointments = await _context.RendezVous
                .Where(r => r.MedecinId == medecinId)
                .Select(r => r.PatientId)
                .ToListAsync();

            var idsFromConsultations = await _context.Consultations
                .Where(c => c.MedecinId == medecinId)
                .Select(c => c.PatientId)
                .ToListAsync();

            return idsFromAppointments
                .Concat(idsFromConsultations)
                .Distinct()
                .ToList();
        }

        private bool IsAdminOrSecretaire()
        {
            return User.IsInRole(SystemRoles.Admin) || User.IsInRole(SystemRoles.Secretaire);
        }

        private string? GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private async Task<int?> GetCurrentMedecinIdAsync()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            var medecin = await _context.Medecins
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ApplicationUserId == userId);

            return medecin?.Id;
        }

        private async Task<bool> CanAccessAppointmentAsync(RendezVou appointment)
        {
            if (IsAdminOrSecretaire())
                return true;

            if (User.IsInRole(SystemRoles.Medecin))
            {
                var medecinId = await GetCurrentMedecinIdAsync();
                return medecinId.HasValue && appointment.MedecinId == medecinId.Value;
            }

            return false;
        }

        private static AppointmentViewModel MapToViewModel(RendezVou appointment)
        {
            return new AppointmentViewModel
            {
                Id = appointment.Id,
                PatientName = BuildPatientName(appointment.Patient),
                MedecinName = appointment.Medecin?.NomPrenom ?? "Unassigned",
                StartTime = appointment.DateDebut,
                EndTime = appointment.DateFin,
                Location = appointment.SalleId.HasValue ? $"Room {appointment.SalleId}" : "Unassigned",
                Status = appointment.Statut,
                Reason = string.IsNullOrWhiteSpace(appointment.Motif) ? "Not specified" : appointment.Motif
            };
        }

        private static string BuildPatientName(Patient? patient)
        {
            if (patient == null)
                return "Unknown patient";

            var parts = new[] { patient.Prenom, patient.Nom }
                .Where(p => !string.IsNullOrWhiteSpace(p));

            var fullName = string.Join(" ", parts);
            return string.IsNullOrWhiteSpace(fullName) ? "Unnamed patient" : fullName;
        }

        private static bool IsCompleted(RendezVou appointment)
        {
            return appointment.Statut?.Equals("Completed", StringComparison.OrdinalIgnoreCase) == true;
        }

        private async Task<IEnumerable<SelectListItem>> GetPatientsSelectListAsync(int? selectedId = null, int? medecinId = null)
        {
            var patients = await GetScopedPatientsAsync(medecinId);

            return patients.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = BuildPatientName(p),
                Selected = selectedId.HasValue && p.Id == selectedId
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetMedecinsSelectListAsync(int? selectedId = null, int? medecinId = null)
        {
            if (IsAdminOrSecretaire())
            {
                var medecins = await _context.Medecins
                    .OrderBy(m => m.NomPrenom)
                    .ToListAsync();

                return medecins.Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(m.NomPrenom) ? "Unnamed provider" : m.NomPrenom,
                    Selected = selectedId.HasValue && m.Id == selectedId
                });
            }

            if (User.IsInRole(SystemRoles.Medecin) && medecinId.HasValue)
            {
                var medecin = await _context.Medecins.FindAsync(medecinId.Value);

                if (medecin != null)
                {
                    return new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value = medecin.Id.ToString(),
                            Text = string.IsNullOrWhiteSpace(medecin.NomPrenom) ? "Médecin assigné" : medecin.NomPrenom,
                            Selected = true
                        }
                    };
                }
            }

            return Enumerable.Empty<SelectListItem>();
        }

        private async Task PopulateDropdownsAsync(ScheduleFormViewModel viewModel, int? medecinId)
        {
            viewModel.Patients = await GetPatientsSelectListAsync(viewModel.Appointment.PatientId, medecinId);
            viewModel.Medecins = await GetMedecinsSelectListAsync(viewModel.Appointment.MedecinId, medecinId);
        }

        private static readonly string[] AllowedStatuses = new[] { "Scheduled", "Checked-in", "Completed" };

        private async Task ValidateAppointmentAsync(RendezVou appointment, int? medecinId)
        {
            if (appointment.DateFin <= appointment.DateDebut)
            {
                ModelState.AddModelError("Appointment.DateFin", "End time must be after the start time.");
            }

            if (string.IsNullOrWhiteSpace(appointment.Statut))
            {
                appointment.Statut = "Scheduled";
            }

            if (IsAdminOrSecretaire())
            {
                if (appointment.PatientId <= 0 || !await _context.Patients.AnyAsync(p => p.Id == appointment.PatientId))
                {
                    ModelState.AddModelError("Appointment.PatientId", "Please select an existing patient.");
                }

                if (appointment.MedecinId <= 0 || !await _context.Medecins.AnyAsync(m => m.Id == appointment.MedecinId))
                {
                    ModelState.AddModelError("Appointment.MedecinId", "Please select an existing provider.");
                }

                return;
            }

            if (User.IsInRole(SystemRoles.Medecin))
            {
                if (!medecinId.HasValue)
                {
                    ModelState.AddModelError(string.Empty, "Your provider profile could not be resolved.");
                    return;
                }

                appointment.MedecinId = medecinId.Value;

                if (appointment.PatientId <= 0)
                {
                    ModelState.AddModelError("Appointment.PatientId", "Please select an existing patient.");
                    return;
                }

                var allowedPatientIds = await GetMedecinPatientIdsAsync(medecinId.Value);
                if (!allowedPatientIds.Contains(appointment.PatientId))
                {
                    ModelState.AddModelError("Appointment.PatientId", "You can only manage appointments for patients assigned to you.");
                }
            }
        }

        private async Task<bool> AppointmentExists(int id)
        {
            return await _context.RendezVous.AnyAsync(e => e.Id == id);
        }

        private void SetSchedulePageMetadata(string title)
        {
            ViewData["Title"] = title;
            ViewData["Breadcrumb"] = "Schedule";
        }

        private static string BuildIcsContent(RendezVou appointment)
        {
            string Escape(string? value) =>
                (value ?? string.Empty)
                    .Replace("\\", "\\\\")
                    .Replace(";", "\\;")
                    .Replace(",", "\\,")
                    .Replace("\n", "\\n");

            var builder = new StringBuilder();
            builder.AppendLine("BEGIN:VCALENDAR");
            builder.AppendLine("VERSION:2.0");
            builder.AppendLine("PRODID:-//MedicalOfficeManagement//Appointments//EN");
            builder.AppendLine("CALSCALE:GREGORIAN");
            builder.AppendLine("METHOD:PUBLISH");
            builder.AppendLine("BEGIN:VEVENT");
            builder.AppendLine($"UID:appointment-{appointment.Id}@medicaloffice.local");
            builder.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
            builder.AppendLine($"DTSTART:{appointment.DateDebut.ToUniversalTime():yyyyMMddTHHmmssZ}");
            builder.AppendLine($"DTEND:{appointment.DateFin.ToUniversalTime():yyyyMMddTHHmmssZ}");
            builder.AppendLine($"SUMMARY:{Escape(BuildPatientName(appointment.Patient))}");
            builder.AppendLine($"DESCRIPTION:{Escape(appointment.Motif ?? "Appointment")}");
            builder.AppendLine($"LOCATION:{Escape(appointment.SalleId.HasValue ? $"Room {appointment.SalleId}" : "Unassigned")}");
            builder.AppendLine($"STATUS:{Escape(appointment.Statut)}");
            builder.AppendLine("END:VEVENT");
            builder.AppendLine("END:VCALENDAR");
            return builder.ToString();
        }
    }

    public class RescheduleRequest
    {
        public int Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }

    public class UpdateStatusRequest
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
