using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public SearchController(MedicalOfficeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? q, string? type)
        {
            ViewData["Title"] = "Search";
            ViewData["Breadcrumb"] = "Search";

            if (string.IsNullOrWhiteSpace(q))
            {
                ViewData["SearchTerm"] = "";
                ViewData["SearchType"] = type ?? "all";
                return View(new GlobalSearchViewModel
                {
                    SearchTerm = "",
                    SearchType = type ?? "all",
                    Patients = new List<PatientSearchResult>(),
                    Appointments = new List<AppointmentSearchResult>()
                });
            }

            var searchTerm = q.Trim();
            var searchType = type ?? "all";
            var searchLower = searchTerm.ToLower();

            var viewModel = new GlobalSearchViewModel
            {
                SearchTerm = searchTerm,
                SearchType = searchType
            };

            // Search patients
            if (searchType == "all" || searchType == "patients")
            {
                // Patients cannot search for other patients
                if (User.IsInRole(SystemRoles.Patient))
                {
                    viewModel.Patients = new List<PatientSearchResult>();
                }
                else
                {
                    var patientQuery = _context.Patients
                        .Where(p => !p.IsDeleted &&
                            ((p.Nom != null && p.Nom.ToLower().Contains(searchLower)) ||
                             (p.Prenom != null && p.Prenom.ToLower().Contains(searchLower)) ||
                             (p.Email != null && p.Email.ToLower().Contains(searchLower)) ||
                             (p.Telephone != null && p.Telephone.Contains(searchTerm))));

                    // Apply role-based filtering
                    if (User.IsInRole(SystemRoles.Medecin))
                    {
                        var medecinId = await GetCurrentMedecinIdAsync();
                        if (medecinId.HasValue)
                        {
                            patientQuery = patientQuery.Where(p =>
                                _context.RendezVous.Any(r => r.PatientId == p.Id && r.MedecinId == medecinId.Value) ||
                                _context.Consultations.Any(c => c.PatientId == p.Id && c.MedecinId == medecinId.Value));
                        }
                        else
                        {
                            patientQuery = patientQuery.Where(_ => false);
                        }
                    }

                    var patients = await patientQuery
                        .OrderBy(p => p.Nom)
                        .ThenBy(p => p.Prenom)
                        .Take(50)
                        .ToListAsync();

                    viewModel.Patients = patients.Select(p => new PatientSearchResult
                    {
                        Id = p.Id,
                        FullName = $"{p.Prenom} {p.Nom}".Trim(),
                        Email = p.Email,
                        Telephone = p.Telephone,
                        DateOfBirth = p.DateNaissance
                    }).ToList();
                }
            }

            // Search appointments
            if (searchType == "all" || searchType == "appointments")
            {
                var appointmentQuery = _context.RendezVous
                    .Include(r => r.Patient)
                    .Include(r => r.Medecin)
                    .Where(r =>
                        ((r.Patient.Nom != null && r.Patient.Nom.ToLower().Contains(searchLower)) ||
                         (r.Patient.Prenom != null && r.Patient.Prenom.ToLower().Contains(searchLower)) ||
                         (r.Medecin.NomPrenom != null && r.Medecin.NomPrenom.ToLower().Contains(searchLower)) ||
                         (r.Motif != null && r.Motif.ToLower().Contains(searchLower))));

                // Apply role-based filtering
                if (User.IsInRole(SystemRoles.Medecin))
                {
                    var medecinId = await GetCurrentMedecinIdAsync();
                    if (medecinId.HasValue)
                    {
                        appointmentQuery = appointmentQuery.Where(r => r.MedecinId == medecinId.Value);
                    }
                    else
                    {
                        appointmentQuery = appointmentQuery.Where(_ => false);
                    }
                }
                else if (!IsAdminOrSecretaire())
                {
                    appointmentQuery = appointmentQuery.Where(_ => false);
                }

                var appointments = await appointmentQuery
                    .OrderByDescending(r => r.DateDebut)
                    .Take(50)
                    .ToListAsync();

                viewModel.Appointments = appointments.Select(r => new AppointmentSearchResult
                {
                    Id = r.Id,
                    PatientName = $"{r.Patient.Prenom} {r.Patient.Nom}".Trim(),
                    DoctorName = r.Medecin?.NomPrenom ?? "Unassigned",
                    StartTime = r.DateDebut,
                    EndTime = r.DateFin,
                    Status = r.Statut,
                    Reason = r.Motif,
                    Room = r.SalleId.HasValue ? $"Room {r.SalleId}" : "Unassigned"
                }).ToList();
            }

            ViewData["SearchTerm"] = searchTerm;
            ViewData["SearchType"] = searchType;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> QuickSearch(string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Json(new { patients = Array.Empty<object>(), appointments = Array.Empty<object>() });
            }

            var searchLower = q.Trim().ToLower();

            // Quick patient search - Patients cannot search for other patients
            List<object> patients;
            if (User.IsInRole(SystemRoles.Patient))
            {
                patients = new List<object>();
            }
            else
            {
                var patientQuery = _context.Patients
                    .Where(p => !p.IsDeleted &&
                        ((p.Nom != null && p.Nom.ToLower().Contains(searchLower)) ||
                         (p.Prenom != null && p.Prenom.ToLower().Contains(searchLower)) ||
                         (p.Email != null && p.Email.ToLower().Contains(searchLower))));

                if (User.IsInRole(SystemRoles.Medecin))
                {
                    var medecinId = await GetCurrentMedecinIdAsync();
                    if (medecinId.HasValue)
                    {
                        patientQuery = patientQuery.Where(p =>
                            _context.RendezVous.Any(r => r.PatientId == p.Id && r.MedecinId == medecinId.Value) ||
                            _context.Consultations.Any(c => c.PatientId == p.Id && c.MedecinId == medecinId.Value));
                    }
                    else
                    {
                        patientQuery = patientQuery.Where(_ => false);
                    }
                }

                var patientResults = await patientQuery
                    .OrderBy(p => p.Nom)
                    .ThenBy(p => p.Prenom)
                    .Take(10)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = $"{p.Prenom} {p.Nom}".Trim(),
                        type = "patient",
                        url = Url.Action("Details", "Patients", new { id = p.Id })
                    })
                    .ToListAsync();

                patients = patientResults.Cast<object>().ToList();
            }

            // Quick appointment search
            var appointmentQuery = _context.RendezVous
                .Include(r => r.Patient)
                .Include(r => r.Medecin)
                .Where(r =>
                    ((r.Patient.Nom != null && r.Patient.Nom.ToLower().Contains(searchLower)) ||
                     (r.Patient.Prenom != null && r.Patient.Prenom.ToLower().Contains(searchLower)) ||
                     (r.Medecin.NomPrenom != null && r.Medecin.NomPrenom.ToLower().Contains(searchLower))));

            if (User.IsInRole(SystemRoles.Medecin))
            {
                var medecinId = await GetCurrentMedecinIdAsync();
                if (medecinId.HasValue)
                {
                    appointmentQuery = appointmentQuery.Where(r => r.MedecinId == medecinId.Value);
                }
                else
                {
                    appointmentQuery = appointmentQuery.Where(_ => false);
                }
            }
            else if (!IsAdminOrSecretaire())
            {
                appointmentQuery = appointmentQuery.Where(_ => false);
            }

            var appointments = await appointmentQuery
                .OrderByDescending(r => r.DateDebut)
                .Take(10)
                .Select(r => new
                {
                    id = r.Id,
                    name = $"{r.Patient.Prenom} {r.Patient.Nom} - {r.DateDebut:MMM dd, yyyy HH:mm}".Trim(),
                    type = "appointment",
                    url = Url.Action("Details", "Appointments", new { id = r.Id })
                })
                .ToListAsync();

            return Json(new { patients, appointments });
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
    }
}
