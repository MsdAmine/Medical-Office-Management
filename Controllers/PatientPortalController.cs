using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public IActionResult Index(string? statusMessage = null)
        {
            return RedirectToAction(nameof(Upcoming), new { statusMessage });
        }

        [HttpGet]
        public async Task<IActionResult> Upcoming(string? statusMessage = null)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return Forbid();

            var viewModel = await BuildPortalViewModelAsync(patient, null, statusMessage);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return Forbid();

            var viewModel = await BuildPortalViewModelAsync(patient);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> RequestAppointment()
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return Forbid();

            var viewModel = await BuildPortalViewModelAsync(patient);
            return View("Request", viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Treatments()
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return Forbid();

            var viewModel = await BuildPortalViewModelAsync(patient);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Results()
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return Forbid();

            var viewModel = await BuildPortalViewModelAsync(patient);
            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Timeline()
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return Forbid();

            var patientId = patient.Id;
            var timelineItems = new List<TimelineItemViewModel>();

            // Get appointments
            var appointments = await _context.RendezVous
                .AsNoTracking()
                .Include(r => r.Medecin)
                .Where(r => r.PatientId == patientId)
                .ToListAsync();

            foreach (var appointment in appointments)
            {
                var isUpcoming = appointment.DateDebut >= DateTime.UtcNow;
                timelineItems.Add(new TimelineItemViewModel
                {
                    Id = $"appt-{appointment.Id}",
                    Date = appointment.DateDebut,
                    Type = "appointment",
                    Title = $"Appointment with {appointment.Medecin?.NomPrenom ?? "Doctor"}",
                    Description = string.IsNullOrWhiteSpace(appointment.Motif) ? "Medical appointment" : appointment.Motif,
                    Icon = isUpcoming ? "fa-calendar-check" : "fa-calendar",
                    Color = isUpcoming ? "blue" : "slate",
                    Status = appointment.Statut ?? "Scheduled",
                    LinkUrl = null
                });
            }

            // Get consultations
            var consultations = await _context.Consultations
                .AsNoTracking()
                .Include(c => c.Medecin)
                .Where(c => c.PatientId == patientId)
                .ToListAsync();

            foreach (var consultation in consultations)
            {
                timelineItems.Add(new TimelineItemViewModel
                {
                    Id = $"consult-{consultation.Id}",
                    Date = consultation.DateConsult,
                    Type = "consultation",
                    Title = $"Consultation with {consultation.Medecin?.NomPrenom ?? "Doctor"}",
                    Description = string.IsNullOrWhiteSpace(consultation.Diagnostics) 
                        ? (string.IsNullOrWhiteSpace(consultation.Observations) ? "Medical consultation" : consultation.Observations)
                        : consultation.Diagnostics,
                    Icon = "fa-stethoscope",
                    Color = "emerald",
                    Status = "Completed",
                    LinkUrl = null
                });
            }

            // Get lab results
            var labResults = await _context.LabResults
                .AsNoTracking()
                .Include(l => l.Medecin)
                .Where(l => l.PatientId == patientId)
                .ToListAsync();

            foreach (var lab in labResults)
            {
                timelineItems.Add(new TimelineItemViewModel
                {
                    Id = $"lab-{lab.Id}",
                    Date = lab.CollectedOn,
                    Type = "lab",
                    Title = string.IsNullOrWhiteSpace(lab.TestName) ? "Lab Test" : lab.TestName,
                    Description = string.IsNullOrWhiteSpace(lab.Notes) ? "Laboratory test results" : lab.Notes,
                    Icon = "fa-flask",
                    Color = "purple",
                    Status = lab.Status ?? "Pending",
                    LinkUrl = null
                });
            }

            // Get prescriptions
            var prescriptions = await _context.Prescriptions
                .AsNoTracking()
                .Include(p => p.Medecin)
                .Where(p => p.PatientId == patientId)
                .ToListAsync();

            foreach (var prescription in prescriptions)
            {
                timelineItems.Add(new TimelineItemViewModel
                {
                    Id = $"presc-{prescription.Id}",
                    Date = prescription.IssuedOn,
                    Type = "prescription",
                    Title = $"Prescription: {prescription.Medication}",
                    Description = $"Dosage: {prescription.Dosage ?? "N/A"}, Frequency: {prescription.Frequency ?? "N/A"}. {(string.IsNullOrWhiteSpace(prescription.Notes) ? "" : prescription.Notes)}",
                    Icon = "fa-prescription-bottle-alt",
                    Color = "amber",
                    Status = prescription.Status ?? "Active",
                    LinkUrl = null
                });
            }

            // Get billing invoices
            var invoices = await _context.BillingInvoices
                .AsNoTracking()
                .Where(i => i.PatientId == patientId)
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                timelineItems.Add(new TimelineItemViewModel
                {
                    Id = $"invoice-{invoice.Id}",
                    Date = invoice.IssuedOn,
                    Type = "billing",
                    Title = $"Invoice #{invoice.InvoiceNumber}",
                    Description = $"{invoice.Service} - {invoice.Amount:C}",
                    Icon = "fa-file-invoice-dollar",
                    Color = "rose",
                    Status = invoice.Status ?? "Draft",
                    LinkUrl = null
                });
            }

            // Sort by date descending (most recent first)
            timelineItems = timelineItems.OrderByDescending(t => t.Date).ToList();

            var viewModel = new TimelineViewModel
            {
                PatientProfile = patient,
                TimelineItems = timelineItems
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> AppointmentStatus(int id)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return Forbid();

            var appointment = await _context.RendezVous
                .AsNoTracking()
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
            if (patient == null) return Forbid();

            var patientId = patient.Id;

            var labResults = await _context.LabResults
                .AsNoTracking()
                .Include(l => l.Medecin)
                .Where(l => l.PatientId == patientId)
                .OrderByDescending(l => l.CollectedOn)
                .Select(l => new PatientResultViewModel
                {
                    Id = l.Id,
                    Title = string.IsNullOrWhiteSpace(l.TestName) ? "Résultat de test" : l.TestName,
                    Status = string.IsNullOrWhiteSpace(l.Status) ? "En attente" : l.Status,
                    Date = l.CollectedOn,
                    OrderedBy = l.Medecin != null && !string.IsNullOrWhiteSpace(l.Medecin.NomPrenom)
                        ? l.Medecin.NomPrenom
                        : "Laboratoire",
                    Notes = string.IsNullOrWhiteSpace(l.Notes)
                        ? "Consultez votre médecin pour plus de détails."
                        : l.Notes!
                })
                .ToListAsync();

            return Ok(labResults);
        }

        [HttpPost]
        public async Task<IActionResult> RequestRefill(int prescriptionId)
        {
            var patient = await GetCurrentPatientAsync();
            if (patient == null) return Forbid();

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
            if (patient == null) return Forbid();

            ValidateAppointmentRequest(request);

            if (!ModelState.IsValid)
            {
                var invalidViewModel = await BuildPortalViewModelAsync(patient, request);
                return View("Request", invalidViewModel);
            }

            var rendezVous = new RendezVou
            {
                PatientId = patient.Id,
                MedecinId = request.MedecinId,
                DateDebut = request.PreferredStart,
                DateFin = request.PreferredEnd,
                Statut = "Pending Approval",
                Motif = request.Reason
            };

            _context.RendezVous.Add(rendezVous);
            await _context.SaveChangesAsync();

            // IMPORTANT: Redirect to the GET action you actually have ("RequestAppointment")
            return RedirectToAction(nameof(RequestAppointment),
                new { statusMessage = "Votre demande de rendez-vous a été envoyée." });
        }

        private async Task<Patient?> GetCurrentPatientAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            return await _context.Patients
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);
        }

        private async Task<PatientPortalViewModel> BuildPortalViewModelAsync(
            Patient patient,
            AppointmentRequestInput? request = null,
            string? statusMessage = null)
        {
            var patientId = patient.Id;
            var now = DateTime.UtcNow;

            var appointments = await _context.RendezVous
                .AsNoTracking()
                .Include(r => r.Medecin)
                .Where(r => r.PatientId == patientId)
                .OrderBy(r => r.DateDebut)
                .ToListAsync();

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
                .AsNoTracking()
                .Include(c => c.Medecin)
                .Where(c => c.PatientId == patientId)
                .OrderByDescending(c => c.DateConsult)
                .ToListAsync();

            var treatments = consultations
                .Select(c => new PatientTreatmentViewModel
                {
                    Title = string.IsNullOrWhiteSpace(c.Diagnostics) ? "Suivi médical" : c.Diagnostics,
                    PrescribedBy = c.Medecin != null && !string.IsNullOrWhiteSpace(c.Medecin.NomPrenom)
                        ? c.Medecin.NomPrenom
                        : "Médecin",
                    Date = c.DateConsult,
                    Notes = string.IsNullOrWhiteSpace(c.Observations) ? "Aucune note fournie." : c.Observations,
                    Status = "En cours"
                })
                .ToList();

            var labResults = await _context.LabResults
                .AsNoTracking()
                .Include(l => l.Medecin)
                .Where(l => l.PatientId == patientId)
                .OrderByDescending(l => l.CollectedOn)
                .Select(l => new PatientResultViewModel
                {
                    Id = l.Id,
                    Title = string.IsNullOrWhiteSpace(l.TestName) ? "Résultat de test" : l.TestName,
                    Status = string.IsNullOrWhiteSpace(l.Status) ? "En attente" : l.Status,
                    Date = l.CollectedOn,
                    OrderedBy = l.Medecin != null && !string.IsNullOrWhiteSpace(l.Medecin.NomPrenom)
                        ? l.Medecin.NomPrenom
                        : "Laboratoire",
                    Notes = string.IsNullOrWhiteSpace(l.Notes)
                        ? "Consultez votre médecin pour plus de détails."
                        : l.Notes!
                })
                .ToListAsync();

            var medecinsList = await _context.Medecins
                .AsNoTracking()
                .OrderBy(m => m.NomPrenom)
                .ToListAsync();

            var medecins = medecinsList
                .Select(m => new SelectListItem
                {
                    Value = m.Id.ToString(),
                    Text = FormatDoctorNameWithSpecialty(m.NomPrenom, m.Specialite),
                    Selected = request != null && request.MedecinId == m.Id
                })
                .ToList();

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
                MedecinName = appointment.Medecin != null && !string.IsNullOrWhiteSpace(appointment.Medecin.NomPrenom)
                    ? appointment.Medecin.NomPrenom
                    : "Non assigné",
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
                ModelState.AddModelError(nameof(request.PreferredEnd),
                    "L'heure de fin doit être après l'heure de début.");
            }

            if (request.PreferredStart < DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(request.PreferredStart),
                    "La date de début doit être dans le futur.");
            }
        }

        private static string FormatDoctorNameWithSpecialty(string? nomPrenom, string? specialite)
        {
            var name = string.IsNullOrWhiteSpace(nomPrenom) ? "Médecin" : nomPrenom.Trim();
            var specialty = string.IsNullOrWhiteSpace(specialite) ? null : specialite.Trim();
            
            // Add "Dr" prefix if not already present
            if (!name.StartsWith("Dr", StringComparison.OrdinalIgnoreCase) && 
                !name.StartsWith("Dr.", StringComparison.OrdinalIgnoreCase))
            {
                name = $"Dr {name}";
            }
            
            // Append specialty in parentheses if available
            if (!string.IsNullOrWhiteSpace(specialty))
            {
                return $"{name} ({specialty})";
            }
            
            return name;
        }
    }
}
