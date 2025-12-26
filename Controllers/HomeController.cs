using System.Diagnostics;
using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MedicalOfficeManagement.Data; 
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize] // Force la redirection vers le Login si l'utilisateur n'est pas connecté
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MedicalOfficeContext _context;

        public HomeController(ILogger<HomeController> logger, MedicalOfficeContext context)
        {
            _logger = logger;
            _context = context; // Injection de la base de données
        }

        public async Task<IActionResult> Index()
        {
            // 1. Récupération du nombre réel de médecins en base de données
            // Note: Assurez-vous que votre DbSet s'appelle bien "Medecins" dans MedicalOfficeContext
            ViewBag.NbMedecins = await _context.Medecins.CountAsync();

            // 2. Données de test (À remplacer par des requêtes réelles une fois vos tables prêtes)
            // Exemple : ViewBag.NbPatients = await _context.Patients.CountAsync();
            ViewBag.NbPatients = 25; 
            ViewBag.NbRendezVous = 12;

            // 3. On retourne la vue Dashboard
            return View();
        }

        [AllowAnonymous] // Permet d'afficher la page même si on n'est pas loggé (ex: erreur 404)
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