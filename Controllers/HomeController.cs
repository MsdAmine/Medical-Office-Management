using System.Diagnostics;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MedicalOfficeManagement.Data; 
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize] // Exige la connexion pour accéder au dashboard
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MedicalOfficeContext _context;

        public HomeController(ILogger<HomeController> logger, MedicalOfficeContext context)
        {
            _logger = logger;
            _context = context; // Injection du contexte de base de données
        }

        public async Task<IActionResult> Index()
        {
            // 1. Compter les médecins réels
            ViewBag.NbDoctors = await _context.Medecins.CountAsync();

            // 2. Simuler des données pour les tables non encore créées (Patients/RDV)
            // Remplacez par _context.Patients.CountAsync() quand vos tables seront prêtes.
            ViewBag.NbPatients = 25; 
            ViewBag.NbAppointments = 12;

            return View();
        }

        [AllowAnonymous] // La page Privacy reste accessible à tous
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}