using System;
using System.Linq;
using System.Security.Claims;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.Patient)]
    public class PatientPortalController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public PatientPortalController(MedicalOfficeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? statusMessage = null)
        {
            return RedirectToAction(nameof(Upcoming), new { statusMessage });
        }

        [HttpGet]
        public async Task<IActionResult> Upcoming(string? statusMessage = null)
        {
            var patient = await GetCurrentPatientAsync();
            var viewModel = await BuildPortalViewModelAsync(patient, null, statusMessage);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var patient = await GetCurrentPatientAsync();
            var viewModel = await BuildPortalViewModelAsync(patient);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Request()
        {
            var patient = await GetCurrentPatientAsync();
            var viewModel = await BuildPortalViewModelAsync(patient);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Treatments()
        {
            var patient = await GetCurrentPatientAsync();
            var viewModel = await BuildPortalViewModelAsync(patient);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Results()
        {
            var patient = await GetCurrentPatientAsync();
            var viewModel = await BuildPortalViewModelAsync(patient);

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AppointmentStatus(int id)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null)
                return Forbid();

            var appointment = await _context.RendezVous
                .Include(r => r.Medecin)
                .FirstOrDefaultAsync(r => r.Id == id && r.PatientId == patient.Id);

            if (appointment == null)
                return NotFound();

            return Ok(MapAppointment(appointment));
        }

        [HttpGet]
        public async Task<IActionResult> LabNotifications()
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null)
                return Forbid();

            var labResults = await _context.LabResults
                .Include(l => l.Medecin)
                .Where(l => l.PatientId == patient.Id)
                .OrderByDescending(l => l.CollectedOn)
                .Select(l => new PatientResultViewModel
                {
                    Id = l.Id,
                    Title = string.IsNullOrWhiteSpace(l.TestName) ? "Résultat de test" : l.TestName,
                    Status = string.IsNullOrWhiteSpace(l.Status) ? "En attente" : l.Status,
                    Date = l.CollectedOn,
                    OrderedBy = l.Medecin?.NomPrenom ?? "Laboratoire",
                    Notes = string.IsNullOrWhiteSpace(l.Notes) ? "Consultez votre médecin pour plus de détails." : l.Notes!
                })
                .ToListAsync();

            return Ok(labResults);
        }

        [HttpPost]
        public async Task<IActionResult> RequestRefill(int prescriptionId)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null)
                return Forbid();

            var prescription = await _context.Prescriptions
                .FirstOrDefaultAsync(p => p.Id == prescriptionId);

            if (prescription == null || prescription.PatientId != patient.Id)
                return NotFound();

            if (prescription.RefillsRemaining <= 0)
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Aucun renouvellement disponible",
                    Detail = "Il n'y a plus de renouvellements disponibles pour cette ordonnance."
                });
            }

            prescription.Status = "Refill Requested";
            prescription.NextRefill ??= DateTime.UtcNow.AddDays(7);

            _context.Prescriptions.Update(prescription);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Votre demande de renouvellement a été envoyée." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestAppointment(AppointmentRequestInput request)
        {
            var patient = await GetCurrentPatientAsync();

            if (patient == null)
            {
                return Forbid();
            }

            ValidateAppointmentRequest(request);

            if (!ModelState.IsValid)
            {
                var invalidViewModel = await BuildPortalViewModelAsync(patient, request);
                return View("Request", invalidViewModel);
            }

            var rendezVous = new RendezVou
            {
                PatientId = patient!.Id,
                MedecinId = request.MedecinId,
                DateDebut = request.PreferredStart,
                DateFin = request.PreferredEnd,
                Statut = "Pending Approval",
                Motif = request.Reason
            };

            _context.RendezVous.Add(rendezVous);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Request), new { statusMessage = "Votre demande de rendez-vous a été envoyée." });
        }

        private async Task<Patient?> GetCurrentPatientAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _context.Patients
                .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);
        }

        private async Task<PatientPortalViewModel> BuildPortalViewModelAsync(
            Patient? patient,
            AppointmentRequestInput? request = null,
            string? statusMessage = null)
        {
            var appointments = await _context.RendezVous
                .Include(r => r.Medecin)
                .Where(r => patient != null && r.PatientId == patient.Id)
                .OrderBy(r => r.DateDebut)
                .ToListAsync();

            var now = DateTime.UtcNow;

            var upcoming = appointments
                .Where(a => a.DateDebut >= now)
                .Select(MapAppointment)
                .ToList();

            var past = appointments
                .Where(a => a.DateDebut < now)
                .OrderByDescending(a => a.DateDebut)
                .Select(MapAppointment)
                .ToList();

            var consultations = await _context.Consultations
                .Include(c => c.Medecin)
                .Where(c => patient != null && c.PatientId == patient.Id)
                .OrderByDescending(c => c.DateConsult)
                .ToListAsync();

            var treatments = consultations
                .Select(c => new PatientTreatmentViewModel
                {
                    Title = string.IsNullOrWhiteSpace(c.Diagnostics) ? "Suivi médical" : c.Diagnostics,
                    PrescribedBy = c.Medecin?.NomPrenom ?? "Médecin",
                    Date = c.DateConsult,
                    Notes = string.IsNullOrWhiteSpace(c.Observations) ? "Aucune note fournie." : c.Observations,
                    Status = "En cours"
                })
                .ToList();

            var labResults = await _context.LabResults
                .Include(l => l.Medecin)
                .Where(l => patient != null && l.PatientId == patient.Id)
                .OrderByDescending(l => l.CollectedOn)
                .Select(l => new PatientResultViewModel
                {
                    Id = l.Id,
                    Title = string.IsNullOrWhiteSpace(l.TestName) ? "Résultat de test" : l.TestName,
                    Status = string.IsNullOrWhiteSpace(l.Status) ? "En attente" : l.Status,
                    Date = l.CollectedOn,
                    OrderedBy = l.Medecin?.NomPrenom ?? "Laboratoire",
                    Notes = string.IsNullOrWhiteSpace(l.Notes) ? "Consultez votre médecin pour plus de détails." : l.Notes!
                })
                .ToListAsync();

            var medecins = await _context.Medecins
                .OrderBy(m => m.NomPrenom)
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(m.NomPrenom) ? "Médecin" : m.NomPrenom,
                    Selected = request != null && request.MedecinId == m.Id
                })
                .ToListAsync();

            return new PatientPortalViewModel
            {
                PatientProfile = patient,
                UpcomingAppointments = upcoming,
                PastAppointments = past,
                Treatments = treatments,
                LabResults = labResults,
                Request = request ?? BuildDefaultRequest(),
                MedecinOptions = medecins,
                StatusMessage = statusMessage
            };
        }

        private static PatientAppointmentViewModel MapAppointment(RendezVou appointment)
        {
            return new PatientAppointmentViewModel
            {
                Id = appointment.Id,
                ScheduledFor = appointment.DateDebut,
                EndsAt = appointment.DateFin,
                MedecinName = appointment.Medecin?.NomPrenom ?? "Non assigné",
                Status = string.IsNullOrWhiteSpace(appointment.Statut) ? "Planifié" : appointment.Statut,
                Reason = string.IsNullOrWhiteSpace(appointment.Motif) ? "Non spécifié" : appointment.Motif
            };
        }

        private static AppointmentRequestInput BuildDefaultRequest()
        {
            var start = DateTime.Today.AddDays(1).AddHours(9);
            return new AppointmentRequestInput
            {
                PreferredStart = start,
                PreferredEnd = start.AddMinutes(30)
            };
        }

        private void ValidateAppointmentRequest(AppointmentRequestInput request)
        {
            if (request.PreferredEnd <= request.PreferredStart)
            {
                ModelState.AddModelError(nameof(request.PreferredEnd), "L'heure de fin doit être après l'heure de début.");
            }

            if (request.PreferredStart < DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(request.PreferredStart), "La date de début doit être dans le futur.");
            }
        }
    }
}
