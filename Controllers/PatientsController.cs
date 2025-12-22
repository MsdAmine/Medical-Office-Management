// File: Controllers/PatientsController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels.Patients;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    public class PatientsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public PatientsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var patients = await _context.Patients
                .Include(p => p.RendezVous)
                .ToListAsync();

            var patientViewModels = patients
                .Select(p =>
                {
                    var lastVisit = p.RendezVous
                        .OrderByDescending(r => r.DateFin != default ? r.DateFin : r.DateDebut)
                        .Select(r => r.DateFin != default ? r.DateFin : r.DateDebut)
                        .FirstOrDefault();

                    var status = CalculatePatientStatus(lastVisit, p.RendezVous.Any());

                    return new PatientViewModel
                    {
                        Id = p.Id,
                        FullName = $"{p.Prenom} {p.Nom}",
                        Phone = p.Telephone ?? "N/A",
                        Email = p.Email ?? "N/A",
                        LastVisit = lastVisit == default ? DateTime.Now : lastVisit,
                        Status = status,
                        StatusColor = MapStatusColor(status)
                    };
                })
                .OrderBy(p => p.FullName)
                .ToList();

            var model = new PatientsIndexViewModel
            {
                Patients = patientViewModels,
                TotalPatients = patientViewModels.Count,
                ActivePatients = patientViewModels.Count(p => p.Status == "Active"),
                NewThisMonth = patientViewModels.Count(p => p.LastVisit.Month == DateTime.Now.Month && p.LastVisit.Year == DateTime.Now.Year)
            };

            return View(model);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        private static string CalculatePatientStatus(DateTime lastVisit, bool hasVisits)
        {
            if (!hasVisits)
            {
                return "Pending";
            }

            var daysSinceLast = (DateTime.Now - (lastVisit == default ? DateTime.Now : lastVisit)).TotalDays;
            if (daysSinceLast <= 60)
            {
                return "Active";
            }

            return "Inactive";
        }

        private static string MapStatusColor(string status)
        {
            return status switch
            {
                "Active" => "bg-green-100 text-green-700 border-green-200",
                "Pending" => "bg-amber-100 text-amber-700 border-amber-200",
                _ => "bg-gray-100 text-gray-700 border-gray-200"
            };
        }
    }
}
