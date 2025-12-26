using System.Diagnostics;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MedicalOfficeManagement.Data; 
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MedicalOfficeContext _context;

        public HomeController(ILogger<HomeController> logger, MedicalOfficeContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Nombre réel de médecins
            ViewBag.NbMedecins = await _context.Medecins.CountAsync();

            // 2. Nombre réel de patients
            ViewBag.NbPatients = await _context.Patients.CountAsync(); 

            // 3. Correction de l'erreur CS1061
            // Note: Si votre propriété s'appelle différemment (ex: DateRdv), remplacez-la ici.
            // Si vous n'avez pas encore de champ date, on initialise à 0 pour éviter le crash.
            try 
            {
                ViewBag.NbRendezVous = await _context.RendezVous
                    .CountAsync(); // Compte total pour le moment pour éviter l'erreur de propriété
            }
            catch 
            {
                ViewBag.NbRendezVous = 0;
            }

            return View();
        }

        [AllowAnonymous] 
        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}