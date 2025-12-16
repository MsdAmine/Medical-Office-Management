using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
// 🛑 CORRECTION 1 : Le DbContext est dans le dossier Models, d'où l'utilisation de l'espace de noms Models
using MedicalOfficeManagement.Models;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MedecinController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MedecinController(MedicalOfficeContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // --- Méthode d'aide pour la liste déroulante ---
        private void PopulateSpecialitesViewBag()
        {
            List<string> specialites = new List<string>
            {
                "Cardiologie", "Dermatologie", "Généraliste",
                "Ophtalmologie", "Pédiatrie", "Radiologie", "Urologie"
            };
            ViewBag.Specialites = new SelectList(specialites);
        }
        // ---------------------------------------------


        // GET: Medecin/Create (Affiche le formulaire)
        public IActionResult Create()
        {
            PopulateSpecialitesViewBag(); // Prépare la liste déroulante
            return View();
        }

        // POST: Medecin/Create (Traite la soumission)
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 🛑 CORRECTION 2 : Mise à jour du [Bind] pour inclure toutes les nouvelles propriétés et NomPrenom
        public async Task<IActionResult> Create([Bind("Id,NomPrenom,Specialite,Adresse,Telephone,Email,ApplicationUserId")] Medecin medecin)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // 🛑 CORRECTION 3 : L'ID est défini avant la vérification ModelState.IsValid (pour les règles EF Core)
            medecin.ApplicationUserId = userId;

            if (ModelState.IsValid)
            {
                _context.Add(medecin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si la validation échoue, nous rechargeons le ViewBag avant de retourner la vue
            PopulateSpecialitesViewBag();
            return View(medecin);
        }

        // GET: Medecin/Index
        public async Task<IActionResult> Index()
        {
            // Inclure l'utilisateur Identity pour l'affichage ou l'accès aux données liées
            var medecins = await _context.Medecins.Include(m => m.ApplicationUser).ToListAsync();
            return View(medecins);
        }

        // 🛑 IMPORTANT : Vous devez également mettre à jour les méthodes Details, Edit, et Delete 
        // dans ce contrôleur pour s'assurer qu'elles incluent les nouvelles propriétés lors de la recherche 
        // et qu'elles utilisent NomPrenom au lieu de Nom/Prenom, mais nous allons d'abord compiler.

        // ... (Vous pouvez ajouter ici les méthodes Details, Edit, Delete si elles existent déjà)
    }
}