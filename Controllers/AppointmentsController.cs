using System;
using System.Linq;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
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
            var appointments = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
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

            var appointment = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
                return NotFound();

            SetSchedulePageMetadata("Appointment Details");
            return View(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new ScheduleFormViewModel
            {
                Appointment = new RendezVou
                {
                    DateDebut = DateTime.Today.AddHours(9),
                    DateFin = DateTime.Today.AddHours(10),
                    Statut = "Scheduled"
                },
                Patients = await GetPatientsSelectListAsync(),
                Medecins = await GetMedecinsSelectListAsync()
            };

            SetSchedulePageMetadata("New Appointment");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleFormViewModel viewModel)
        {
            ValidateAppointmentTimes(viewModel.Appointment);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel);
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

            var viewModel = new ScheduleFormViewModel
            {
                Appointment = appointment,
                Patients = await GetPatientsSelectListAsync(appointment.PatientId),
                Medecins = await GetMedecinsSelectListAsync(appointment.MedecinId)
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

            ValidateAppointmentTimes(viewModel.Appointment);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(viewModel);
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

            var appointment = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
                return NotFound();

            SetSchedulePageMetadata("Delete Appointment");
            return View(appointment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.RendezVous.FindAsync(id);

            if (appointment != null)
            {
                _context.RendezVous.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
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

        private async Task<IEnumerable<SelectListItem>> GetPatientsSelectListAsync(int? selectedId = null)
        {
            var patients = await _context.Patients
                .OrderBy(p => p.Nom)
                .ThenBy(p => p.Prenom)
                .ToListAsync();

            return patients.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = BuildPatientName(p),
                Selected = selectedId.HasValue && p.Id == selectedId
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetMedecinsSelectListAsync(int? selectedId = null)
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

        private async Task PopulateDropdownsAsync(ScheduleFormViewModel viewModel)
        {
            viewModel.Patients = await GetPatientsSelectListAsync(viewModel.Appointment.PatientId);
            viewModel.Medecins = await GetMedecinsSelectListAsync(viewModel.Appointment.MedecinId);
        }

        private void ValidateAppointmentTimes(RendezVou appointment)
        {
            if (appointment.PatientId <= 0)
            {
                ModelState.AddModelError("Appointment.PatientId", "Patient selection is required.");
            }

            if (appointment.MedecinId <= 0)
            {
                ModelState.AddModelError("Appointment.MedecinId", "Provider selection is required.");
            }

            if (appointment.DateFin <= appointment.DateDebut)
            {
                ModelState.AddModelError("Appointment.DateFin", "End time must be after the start time.");
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
    }
}
