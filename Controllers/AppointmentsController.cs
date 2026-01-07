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
using MedicalOfficeManagement.Services.Email;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace MedicalOfficeManagement.Controllers
{


    [Authorize(Roles = SystemRoles.SchedulingTeam)]
    public class AppointmentsController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(MedicalOfficeContext context, IEmailSender emailSender, ILogger<AppointmentsController> logger)
        {
            _context = context;
            _emailSender = emailSender;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? statusFilter, int? medecinFilter, DateTime? dateFromFilter, DateTime? dateToFilter, string? searchTerm, int page = 1)
        {
            const int pageSize = 20;

            _logger.LogInformation("Appointments Index accessed by user {UserId} with filters: Status={StatusFilter}, Medecin={MedecinFilter}, DateFrom={DateFrom}, DateTo={DateTo}, Search={SearchTerm}, Page={Page}",
                GetUserId(), statusFilter, medecinFilter, dateFromFilter, dateToFilter, searchTerm, page);

            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                _logger.LogWarning("Medecin user {UserId} attempted to access appointments without valid medecin profile", GetUserId());
                return Forbid();
            }

            var query = GetScopedAppointmentsQueryable(medecinId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                // Support both enum names and legacy string values
                if (Enum.TryParse<AppointmentStatus>(statusFilter, true, out var statusEnum))
                {
                    var statusString = statusEnum.ToString();
                    query = query.Where(r => r.Statut == statusString || 
                        (statusEnum == AppointmentStatus.Scheduled && (r.Statut == "Scheduled" || r.Statut == "Confirmed")));
                }
                else
                {
                    query = query.Where(r => r.Statut == statusFilter);
                }
            }

            if (medecinFilter.HasValue && (IsAdminOrSecretaire() || medecinFilter.Value == medecinId))
            {
                query = query.Where(r => r.MedecinId == medecinFilter.Value);
            }

            if (dateFromFilter.HasValue)
            {
                query = query.Where(r => r.DateDebut >= dateFromFilter.Value);
            }

            if (dateToFilter.HasValue)
            {
                query = query.Where(r => r.DateDebut <= dateToFilter.Value.AddDays(1).AddTicks(-1));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(r =>
                    (r.Patient.Nom != null && r.Patient.Nom.ToLower().Contains(searchLower)) ||
                    (r.Patient.Prenom != null && r.Patient.Prenom.ToLower().Contains(searchLower)) ||
                    (r.Medecin.NomPrenom != null && r.Medecin.NomPrenom.ToLower().Contains(searchLower)) ||
                    (r.Motif != null && r.Motif.ToLower().Contains(searchLower)));
            }

            var orderedQuery = query.OrderBy(r => r.DateDebut);

            // Get total count
            var totalCount = await orderedQuery.CountAsync();
            
            // Get items for current page
            var items = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var appointmentViewModels = items
                .Select(MapToViewModel)
                .ToList();

            // Create IPagedList manually
            var pagedAppointments = new StaticPagedList<AppointmentViewModel>(
                appointmentViewModels,
                page,
                pageSize,
                totalCount);

            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            // Get all appointments for statistics (unfiltered)
            var allAppointments = await GetScopedAppointmentsQueryable(medecinId).ToListAsync();

            var viewModel = new ScheduleIndexViewModel
            {
                UpcomingCount = allAppointments.Count(a => a.DateDebut.Date >= DateTime.Today),
                InClinicToday = allAppointments.Count(a => a.DateDebut.Date == DateTime.Today),
                CompletedThisWeek = allAppointments.Count(a =>
                    IsCompleted(a) &&
                    a.DateDebut >= startOfWeek &&
                    a.DateDebut < endOfWeek),
                PendingApprovalCount = allAppointments.Count(a => 
                    a.StatusEnum == AppointmentStatus.PendingApproval || 
                    a.Statut.Equals("Pending Approval", StringComparison.OrdinalIgnoreCase)),
                Appointments = appointmentViewModels,
                StatusFilter = statusFilter,
                MedecinFilter = medecinFilter,
                DateFromFilter = dateFromFilter,
                DateToFilter = dateToFilter,
                SearchTerm = searchTerm,
                StatusOptions = GetAllowedStatusesSelectList(statusFilter),
                MedecinOptions = await GetMedecinsSelectListAsync(medecinFilter, medecinId)
            };

            // Set pagination view data
            ViewData["PageNumber"] = pagedAppointments.PageNumber;
            ViewData["PageCount"] = pagedAppointments.PageCount;
            ViewData["TotalItemCount"] = pagedAppointments.TotalItemCount;
            ViewData["HasPreviousPage"] = pagedAppointments.HasPreviousPage;
            ViewData["HasNextPage"] = pagedAppointments.HasNextPage;

            SetSchedulePageMetadata("Schedule");

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await BuildAppointmentQuery()
                .Include(r => r.Consultations)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            SetSchedulePageMetadata("Appointment Details");
            return View(appointment);
        }

        [HttpGet]
        public async Task<IActionResult> CreateConsultation(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await BuildAppointmentQuery()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            // Check if consultation already exists for this appointment
            var existingConsultation = await _context.Consultations
                .FirstOrDefaultAsync(c => c.RendezVousId == id);

            if (existingConsultation != null)
            {
                TempData["StatusMessage"] = "A consultation already exists for this appointment.";
                return RedirectToAction("Details", "Consultations", new { id = existingConsultation.Id });
            }

            return RedirectToAction("Create", "Consultations", new { rendezVousId = id });
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? patientId)
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
                    StatusEnum = AppointmentStatus.Scheduled
                },
                Patients = await GetPatientsSelectListAsync(selectedId: patientId, medecinId),
                Medecins = await GetMedecinsSelectListAsync(selectedId: medecinId, medecinId: medecinId)
            };

            if (patientId.HasValue)
            {
                viewModel.Appointment.PatientId = patientId.Value;
            }

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
            _logger.LogInformation("Creating new appointment for patient {PatientId} with doctor {MedecinId} by user {UserId}",
                viewModel.Appointment.PatientId, viewModel.Appointment.MedecinId, GetUserId());

            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                _logger.LogWarning("Medecin user {UserId} attempted to create appointment without valid medecin profile", GetUserId());
                return Forbid();
            }

            await ValidateAppointmentAsync(viewModel.Appointment, medecinId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Appointment creation validation failed. Errors: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                await PopulateDropdownsAsync(viewModel, medecinId);
                SetSchedulePageMetadata("New Appointment");
                return View(viewModel);
            }

            try
            {
                _context.RendezVous.Add(viewModel.Appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} created successfully for patient {PatientId}",
                    viewModel.Appointment.Id, viewModel.Appointment.PatientId);

                TempData["StatusMessage"] = "Appointment created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating appointment for patient {PatientId}", viewModel.Appointment.PatientId);
                ModelState.AddModelError(string.Empty, "An error occurred while creating the appointment. Please try again.");
                await PopulateDropdownsAsync(viewModel, medecinId);
                SetSchedulePageMetadata("New Appointment");
                return View(viewModel);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.RendezVous
            .Include(r => r.Patient)
            .Include(r => r.Medecin)
            .FirstOrDefaultAsync(r => r.Id == id);

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

            var appointment = await _context.RendezVous
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
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

            var previousStatus = appointment.StatusEnum;
            appointment.PatientId = viewModel.Appointment.PatientId;
            appointment.MedecinId = viewModel.Appointment.MedecinId;
            appointment.DateDebut = viewModel.Appointment.DateDebut;
            appointment.DateFin = viewModel.Appointment.DateFin;
            appointment.SalleId = viewModel.Appointment.SalleId;
            appointment.Statut = viewModel.Appointment.Statut;
            appointment.Motif = viewModel.Appointment.Motif;

            _logger.LogInformation("Editing appointment {AppointmentId} by user {UserId}. Status change: {PreviousStatus} -> {NewStatus}",
                appointment.Id, GetUserId(), previousStatus, appointment.StatusEnum);

            await ValidateAppointmentAsync(appointment, medecinId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Appointment {AppointmentId} edit validation failed. Errors: {Errors}",
                    appointment.Id, string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                viewModel.Appointment = appointment;
                await PopulateDropdownsAsync(viewModel, medecinId);
                SetSchedulePageMetadata("Edit Appointment");
                return View(viewModel);
            }

            // Load Patient and Medecin for email sending
            await _context.Entry(appointment).Reference(r => r.Patient).LoadAsync();
            await _context.Entry(appointment).Reference(r => r.Medecin).LoadAsync();

            try
            {
                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} updated successfully", appointment.Id);

                // Send email if appointment was approved (status changed to "Scheduled")
                var newStatus = appointment.StatusEnum;
                if (previousStatus != AppointmentStatus.Scheduled && newStatus == AppointmentStatus.Scheduled)
                {
                    if (!string.IsNullOrWhiteSpace(appointment.Patient?.Email))
                    {
                        try
                        {
                            await _emailSender.SendAsync(
                                appointment.Patient.Email,
                                "Appointment Confirmation",
                                AppointmentEmailTemplates.AppointmentApproved(appointment)
                            );
                            _logger.LogInformation("Approval email sent successfully to {Email} for appointment {AppointmentId}",
                                appointment.Patient.Email, appointment.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send approval email to {Email} for appointment {AppointmentId}", 
                                appointment.Patient?.Email, appointment.Id);
                        }
                    }
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AppointmentExists(viewModel.Appointment.Id))
                {
                    _logger.LogWarning("Appointment {AppointmentId} not found during edit", viewModel.Appointment.Id);
                    return NotFound();
                }

                _logger.LogError("Concurrency exception while editing appointment {AppointmentId}", viewModel.Appointment.Id);
                throw;
            }

            TempData["StatusMessage"] = "Appointment updated successfully.";
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
            _logger.LogInformation("Deleting appointment {AppointmentId} by user {UserId}", id, GetUserId());

            var appointment = await _context.RendezVous.FindAsync(id);

            if (appointment == null)
            {
                _logger.LogWarning("Attempted to delete non-existent appointment {AppointmentId}", id);
                return NotFound();
            }

            if (!await CanAccessAppointmentAsync(appointment))
            {
                _logger.LogWarning("User {UserId} attempted to delete appointment {AppointmentId} without access", GetUserId(), id);
                return Forbid();
            }

            try
            {
                _context.RendezVous.Remove(appointment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Appointment {AppointmentId} deleted successfully", id);
                TempData["StatusMessage"] = "Appointment deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting appointment {AppointmentId}", id);
                TempData["StatusMessage"] = "Error: Failed to delete appointment. Please try again.";
            }

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

            var appointment = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            if (appointment == null)
                return NotFound();

            if (!await CanAccessAppointmentAsync(appointment))
                return Forbid();

            // Validate status using enum
            if (!Enum.TryParse<AppointmentStatus>(request.Status, true, out var newStatusEnum))
            {
                _logger.LogWarning("Invalid status value {Status} provided for appointment {AppointmentId}", request.Status, request.Id);
                return BadRequest("Unsupported status value.");
            }

            var previousStatus = appointment.StatusEnum;
            appointment.StatusEnum = newStatusEnum;
            
            _logger.LogInformation("Updating status of appointment {AppointmentId} from {PreviousStatus} to {NewStatus} by user {UserId}",
                appointment.Id, previousStatus, newStatusEnum, GetUserId());

            await _context.SaveChangesAsync();

            // Send email if appointment was approved (status changed to "Scheduled")
            if (previousStatus != AppointmentStatus.Scheduled && newStatusEnum == AppointmentStatus.Scheduled)
            {
                if (!string.IsNullOrWhiteSpace(appointment.Patient?.Email))
                {
                    try
                    {
                        await _emailSender.SendAsync(
                            appointment.Patient.Email,
                            "Appointment Confirmation",
                            AppointmentEmailTemplates.AppointmentApproved(appointment)
                        );
                        _logger.LogInformation("Approval email sent successfully to {Email} for appointment {AppointmentId}",
                            appointment.Patient.Email, appointment.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send approval email to {Email} for appointment {AppointmentId}", 
                            appointment.Patient?.Email, appointment.Id);
                    }
                }
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> PendingApproval(int? medecinFilter, DateTime? dateFromFilter, DateTime? dateToFilter, string? searchTerm)
        {
            if (!IsAdminOrSecretaire())
            {
                return Forbid();
            }

            var query = BuildAppointmentQuery()
                .Where(r => r.StatusEnum == AppointmentStatus.PendingApproval || 
                    r.Statut.Equals("Pending Approval", StringComparison.OrdinalIgnoreCase));

            // Apply filters
            if (medecinFilter.HasValue)
            {
                query = query.Where(r => r.MedecinId == medecinFilter.Value);
            }

            if (dateFromFilter.HasValue)
            {
                query = query.Where(r => r.DateDebut >= dateFromFilter.Value);
            }

            if (dateToFilter.HasValue)
            {
                query = query.Where(r => r.DateDebut <= dateToFilter.Value.AddDays(1).AddTicks(-1));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(r =>
                    (r.Patient.Nom != null && r.Patient.Nom.ToLower().Contains(searchLower)) ||
                    (r.Patient.Prenom != null && r.Patient.Prenom.ToLower().Contains(searchLower)) ||
                    (r.Medecin.NomPrenom != null && r.Medecin.NomPrenom.ToLower().Contains(searchLower)) ||
                    (r.Motif != null && r.Motif.ToLower().Contains(searchLower)));
            }

            var pending = await query
                .OrderBy(r => r.DateDebut)
                .ToListAsync();

            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var urgentCount = pending.Count(a => a.DateDebut.Date == today || a.DateDebut.Date == tomorrow);

            var viewModel = new PendingApprovalViewModel
            {
                PendingAppointments = pending.Select(MapToViewModel).ToList(),
                TotalPending = pending.Count,
                UrgentCount = urgentCount,
                MedecinFilter = medecinFilter,
                DateFromFilter = dateFromFilter,
                DateToFilter = dateToFilter,
                SearchTerm = searchTerm,
                MedecinOptions = await GetMedecinsSelectListAsync(medecinFilter, null)
            };

            SetSchedulePageMetadata("Pending Approvals");
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApprovePending(int id, int roomNumber)
        {
            if (!IsAdminOrSecretaire())
            {
                return Forbid();
            }

            if (roomNumber < 1)
            {
                TempData["StatusMessage"] = "Error: Room number must be provided and must be greater than 0.";
                return RedirectToAction(nameof(PendingApproval));
            }

            var appointment = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Approving appointment {AppointmentId} with room {RoomNumber} by user {UserId}",
                appointment.Id, roomNumber, GetUserId());

            appointment.StatusEnum = AppointmentStatus.Scheduled;
            appointment.SalleId = roomNumber;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment {AppointmentId} approved successfully", appointment.Id);

            if (!string.IsNullOrWhiteSpace(appointment.Patient?.Email))
            {
                try
                {
                    await _emailSender.SendAsync(
                        appointment.Patient.Email,
                        "Appointment Confirmation",
                        AppointmentEmailTemplates.AppointmentApproved(appointment)
                    );
                    _logger.LogInformation("Confirmation email sent successfully to {Email} for appointment {AppointmentId}", 
                        appointment.Patient.Email, appointment.Id);
                    TempData["StatusMessage"] = "Appointment approved with room assignment and confirmation email sent.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send confirmation email to {Email} for appointment {AppointmentId}", 
                        appointment.Patient.Email, appointment.Id);
                    TempData["StatusMessage"] = "Appointment approved with room assignment. Warning: Failed to send confirmation email.";
                }
            }
            else
            {
                TempData["StatusMessage"] = "Appointment approved with room assignment.";
            }

            return RedirectToAction(nameof(PendingApproval));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeclinePending(int id)
        {
            if (!IsAdminOrSecretaire())
            {
                return Forbid();
            }

            var appointment = await _context.RendezVous.FindAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _logger.LogInformation("Declining appointment {AppointmentId} by user {UserId}", appointment.Id, GetUserId());

            appointment.StatusEnum = AppointmentStatus.Cancelled;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Appointment {AppointmentId} marked as cancelled", appointment.Id);
            TempData["StatusMessage"] = "Appointment marked as cancelled.";
            return RedirectToAction(nameof(PendingApproval));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkApprove([FromForm] string[] appointmentIds, [FromForm] int[] roomNumbers)
        {
            if (!IsAdminOrSecretaire())
            {
                return Forbid();
            }

            if (appointmentIds == null || appointmentIds.Length == 0)
            {
                TempData["StatusMessage"] = "No appointments selected.";
                return RedirectToAction(nameof(PendingApproval));
            }

            if (roomNumbers == null || roomNumbers.Length != appointmentIds.Length)
            {
                TempData["StatusMessage"] = "Error: Room number must be provided for each appointment.";
                return RedirectToAction(nameof(PendingApproval));
            }

            var ids = appointmentIds
                .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToArray();

            if (ids.Length == 0)
            {
                TempData["StatusMessage"] = "No valid appointments selected.";
                return RedirectToAction(nameof(PendingApproval));
            }

            // Validate room numbers
            if (roomNumbers.Any(r => r < 1))
            {
                TempData["StatusMessage"] = "Error: All room numbers must be greater than 0.";
                return RedirectToAction(nameof(PendingApproval));
            }

            var appointments = await _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .Where(r => ids.Contains(r.Id) && 
                    (r.StatusEnum == AppointmentStatus.PendingApproval || 
                     r.Statut.Equals("Pending Approval", StringComparison.OrdinalIgnoreCase)))
                .ToListAsync();

            _logger.LogInformation("Bulk approving {Count} appointments by user {UserId}", appointments.Count, GetUserId());

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var approvedCount = 0;
                var emailFailures = new List<string>();

                for (int i = 0; i < appointments.Count && i < roomNumbers.Length; i++)
                {
                    var appointment = appointments[i];
                    appointment.StatusEnum = AppointmentStatus.Scheduled;
                    appointment.SalleId = roomNumbers[i];
                    approvedCount++;
                    
                    _logger.LogDebug("Approving appointment {AppointmentId} with room {RoomNumber}", appointment.Id, roomNumbers[i]);

                    if (!string.IsNullOrWhiteSpace(appointment.Patient?.Email))
                    {
                        try
                        {
                            await _emailSender.SendAsync(
                                appointment.Patient.Email,
                                "Appointment Confirmation",
                                AppointmentEmailTemplates.AppointmentApproved(appointment)
                            );
                            _logger.LogInformation("Confirmation email sent successfully to {Email} for appointment {AppointmentId}", 
                                appointment.Patient.Email, appointment.Id);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send confirmation email to {Email} for appointment {AppointmentId}", 
                                appointment.Patient.Email, appointment.Id);
                            emailFailures.Add(appointment.Patient.Email ?? "Unknown");
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (emailFailures.Any())
                {
                    TempData["StatusMessage"] = $"{approvedCount} appointment(s) approved with room assignments. " +
                        $"Warning: Failed to send confirmation emails to {emailFailures.Count} recipient(s).";
                }
                else
                {
                    TempData["StatusMessage"] = $"{approvedCount} appointment(s) approved with room assignments and confirmation emails sent.";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during bulk appointment approval");
                TempData["StatusMessage"] = "Error: Failed to approve appointments. Please try again.";
                return RedirectToAction(nameof(PendingApproval));
            }

            return RedirectToAction(nameof(PendingApproval));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDecline([FromForm] string appointmentIds)
        {
            if (!IsAdminOrSecretaire())
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(appointmentIds))
            {
                TempData["StatusMessage"] = "No appointments selected.";
                return RedirectToAction(nameof(PendingApproval));
            }

            var ids = appointmentIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
                .Where(id => id.HasValue)
                .Select(id => id!.Value)
                .ToArray();

            if (ids.Length == 0)
            {
                TempData["StatusMessage"] = "No valid appointments selected.";
                return RedirectToAction(nameof(PendingApproval));
            }

            var appointments = await _context.RendezVous
                .Where(r => ids.Contains(r.Id) && 
                    (r.StatusEnum == AppointmentStatus.PendingApproval || 
                     r.Statut.Equals("Pending Approval", StringComparison.OrdinalIgnoreCase)))
                .ToListAsync();

            _logger.LogInformation("Bulk declining {Count} appointments by user {UserId}", appointments.Count, GetUserId());

            foreach (var appointment in appointments)
            {
                appointment.StatusEnum = AppointmentStatus.Cancelled;
            }

            await _context.SaveChangesAsync();

            TempData["StatusMessage"] = $"{appointments.Count} appointment(s) marked as cancelled.";
            return RedirectToAction(nameof(PendingApproval));
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

        [HttpGet]
        public async Task<IActionResult> ExportCsv(string? statusFilter, int? medecinFilter, DateTime? dateFromFilter, DateTime? dateToFilter, string? searchTerm)
        {
            _logger.LogInformation("CSV export requested by user {UserId} with filters: Status={StatusFilter}, Medecin={MedecinFilter}, DateFrom={DateFrom}, DateTo={DateTo}, Search={SearchTerm}",
                GetUserId(), statusFilter, medecinFilter, dateFromFilter, dateToFilter, searchTerm);

            var medecinId = await GetCurrentMedecinIdAsync();
            if (User.IsInRole(SystemRoles.Medecin) && !medecinId.HasValue)
            {
                return Forbid();
            }

            var query = GetScopedAppointmentsQueryable(medecinId);

            // Apply same filters as Index
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                if (Enum.TryParse<AppointmentStatus>(statusFilter, true, out var statusEnum))
                {
                    var statusString = statusEnum.ToString();
                    query = query.Where(r => r.Statut == statusString || 
                        (statusEnum == AppointmentStatus.Scheduled && (r.Statut == "Scheduled" || r.Statut == "Confirmed")));
                }
                else
                {
                    query = query.Where(r => r.Statut == statusFilter);
                }
            }

            if (medecinFilter.HasValue && (IsAdminOrSecretaire() || medecinFilter.Value == medecinId))
            {
                query = query.Where(r => r.MedecinId == medecinFilter.Value);
            }

            if (dateFromFilter.HasValue)
            {
                query = query.Where(r => r.DateDebut >= dateFromFilter.Value);
            }

            if (dateToFilter.HasValue)
            {
                query = query.Where(r => r.DateDebut <= dateToFilter.Value.AddDays(1).AddTicks(-1));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var searchLower = searchTerm.ToLower();
                query = query.Where(r =>
                    (r.Patient.Nom != null && r.Patient.Nom.ToLower().Contains(searchLower)) ||
                    (r.Patient.Prenom != null && r.Patient.Prenom.ToLower().Contains(searchLower)) ||
                    (r.Medecin.NomPrenom != null && r.Medecin.NomPrenom.ToLower().Contains(searchLower)) ||
                    (r.Motif != null && r.Motif.ToLower().Contains(searchLower)));
            }

            var appointments = await query
                .OrderBy(r => r.DateDebut)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Id,Patient Name,Doctor,Start Time,End Time,Room,Status,Reason");

            foreach (var appointment in appointments)
            {
                var patientName = BuildPatientName(appointment.Patient);
                var doctorName = appointment.Medecin?.NomPrenom ?? "Unassigned";
                var room = appointment.SalleId.HasValue ? appointment.SalleId.ToString() : "Unassigned";
                var status = appointment.StatusEnum.ToString();
                var reason = appointment.Motif ?? "Not specified";

                builder.AppendLine($"{appointment.Id},\"{patientName}\",\"{doctorName}\",{appointment.DateDebut:yyyy-MM-dd HH:mm:ss},{appointment.DateFin:yyyy-MM-dd HH:mm:ss},{room},{status},\"{reason}\"");
            }

            var fileName = $"appointments-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            _logger.LogInformation("CSV export completed. {Count} appointments exported", appointments.Count);

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
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
            return appointment.StatusEnum == AppointmentStatus.Completed;
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
                    Text = FormatDoctorNameWithSpecialty(m.NomPrenom, m.Specialite),
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
                            Text = FormatDoctorNameWithSpecialty(medecin.NomPrenom, medecin.Specialite),
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

        private static IEnumerable<SelectListItem> GetAllowedStatusesSelectList(string? selectedStatus = null)
        {
            var statuses = Enum.GetValues<AppointmentStatus>().Select(e => new
            {
                Value = e.ToString(),
                Text = e.ToString().Replace("PendingApproval", "Pending Approval")
            }).ToList();

            return statuses.Select(s => new SelectListItem
            {
                Value = s.Value,
                Text = s.Text,
                Selected = !string.IsNullOrWhiteSpace(selectedStatus) && 
                    (s.Value.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase) ||
                     s.Text.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase))
            }).Prepend(new SelectListItem { Value = "", Text = "All Statuses", Selected = string.IsNullOrWhiteSpace(selectedStatus) });
        }

        private static readonly AppointmentStatus[] AllowedStatuses = Enum.GetValues<AppointmentStatus>();

        private async Task ValidateAppointmentAsync(RendezVou appointment, int? medecinId)
        {
            if (appointment.DateFin <= appointment.DateDebut)
            {
                ModelState.AddModelError("Appointment.DateFin", "End time must be after the start time.");
            }

            if (string.IsNullOrWhiteSpace(appointment.Statut))
            {
                appointment.StatusEnum = AppointmentStatus.Scheduled;
            }
            else
            {
                appointment.Statut = appointment.Statut.Trim();
                // Ensure status is valid enum
                if (!Enum.TryParse<AppointmentStatus>(appointment.Statut, true, out _))
                {
                    // Try to map legacy values
                    var statusLower = appointment.Statut.ToLowerInvariant();
                    appointment.StatusEnum = statusLower switch
                    {
                        "scheduled" or "confirmed" => AppointmentStatus.Scheduled,
                        "completed" => AppointmentStatus.Completed,
                        "cancelled" => AppointmentStatus.Cancelled,
                        "pending approval" or "pendingapproval" => AppointmentStatus.PendingApproval,
                        "noshow" or "no show" => AppointmentStatus.NoShow,
                        _ => AppointmentStatus.Scheduled
                    };
                }
            }

            if (!AllowedStatuses.Contains(appointment.StatusEnum))
            {
                ModelState.AddModelError("Appointment.Statut", "Please select a supported status value.");
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

                // Check for appointment conflicts
                await CheckAppointmentConflictsAsync(appointment);

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

                // Check for appointment conflicts
                await CheckAppointmentConflictsAsync(appointment);
            }
        }

        private async Task CheckAppointmentConflictsAsync(RendezVou appointment)
        {
            // Statuses that don't count as conflicts (appointments that are finished/cancelled)
            var nonConflictStatuses = new[] 
            { 
                AppointmentStatus.Cancelled, 
                AppointmentStatus.Completed, 
                AppointmentStatus.NoShow 
            };

            // Check for conflicts with the same patient
            var patientConflicts = await _context.RendezVous
                .Where(r => r.Id != appointment.Id && // Exclude current appointment if editing
                           r.PatientId == appointment.PatientId &&
                           !nonConflictStatuses.Contains(r.StatusEnum) &&
                           ((r.DateDebut < appointment.DateFin && r.DateFin > appointment.DateDebut)))
                .AnyAsync();

            if (patientConflicts)
            {
                _logger.LogWarning("Patient conflict detected for appointment {AppointmentId}, patient {PatientId} already has appointment during {StartTime}-{EndTime}",
                    appointment.Id, appointment.PatientId, appointment.DateDebut, appointment.DateFin);
                ModelState.AddModelError("Appointment.DateDebut", "This patient already has an appointment scheduled during this time.");
            }

            // Check for conflicts with the same doctor
            var doctorConflicts = await _context.RendezVous
                .Where(r => r.Id != appointment.Id && // Exclude current appointment if editing
                           r.MedecinId == appointment.MedecinId &&
                           !nonConflictStatuses.Contains(r.StatusEnum) &&
                           ((r.DateDebut < appointment.DateFin && r.DateFin > appointment.DateDebut)))
                .AnyAsync();

            if (doctorConflicts)
            {
                _logger.LogWarning("Doctor conflict detected for appointment {AppointmentId}, doctor {MedecinId} already has appointment during {StartTime}-{EndTime}",
                    appointment.Id, appointment.MedecinId, appointment.DateDebut, appointment.DateFin);
                ModelState.AddModelError("Appointment.DateDebut", "This doctor already has an appointment scheduled during this time.");
            }

            // Check for room conflicts if room is assigned
            if (appointment.SalleId.HasValue)
            {
                var roomConflicts = await _context.RendezVous
                    .Where(r => r.Id != appointment.Id && // Exclude current appointment if editing
                               r.SalleId == appointment.SalleId &&
                               !nonConflictStatuses.Contains(r.StatusEnum) &&
                               ((r.DateDebut < appointment.DateFin && r.DateFin > appointment.DateDebut)))
                    .AnyAsync();

                if (roomConflicts)
                {
                    _logger.LogWarning("Room conflict detected for appointment {AppointmentId}, room {SalleId} already booked during {StartTime}-{EndTime}",
                        appointment.Id, appointment.SalleId, appointment.DateDebut, appointment.DateFin);
                    ModelState.AddModelError("Appointment.SalleId", "This room is already booked during this time.");
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

        private static string FormatDoctorNameWithSpecialty(string? nomPrenom, string? specialite)
        {
            var name = string.IsNullOrWhiteSpace(nomPrenom) ? "Unnamed provider" : nomPrenom.Trim();
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
