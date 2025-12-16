using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims; // Nécessaire pour ClaimTypes.NameIdentifier

namespace MedicalOfficeManagement.Controllers
{
    // Sécurité: Seuls les utilisateurs connectés ayant le rôle "Admin" peuvent accéder à ce contrôleur.
    [Authorize(Roles = "Admin")]
    public class MedecinController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // Injection du DbContext et de UserManager
        public MedecinController(MedicalOfficeContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Medecin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Medecin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nom,Prenom,Specialite")] Medecin medecin)
        {
            // 1. Récupérer l'ID de l'utilisateur
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                // Si l'ID est null (ce qui ne devrait pas arriver avec [Authorize]), rediriger vers le login.
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // 2. Lier la fiche Medecin à l'utilisateur Identity
            medecin.ApplicationUserId = userId;

            // 🛑 VÉRIFICATION CRUCIALE : S'assurer que l'ID existe dans AspNetUsers 🛑
            // L'erreur précédente suggérait un conflit de clé étrangère,
            // indiquant que l'ID de l'utilisateur connecté n'existe pas dans la table AspNetUsers.
            var identityUser = await _userManager.FindByIdAsync(userId);

            if (identityUser == null)
            {
                // Si l'utilisateur n'est pas trouvé, l'INSERT SQL va échouer.
                // Nous bloquons l'opération ici et affichons un message d'erreur.
                ModelState.AddModelError("", $"ERREUR DE CLÉ ÉTRANGÈRE : L'utilisateur Identity avec l'ID '{userId}' (Email: {User.Identity.Name}) n'a pas été trouvé dans la table AspNetUsers. Veuillez vérifier votre DbInitializer ou recréer l'utilisateur Admin.");
                return View(medecin);
            }
            // 🛑 FIN VÉRIFICATION CRUCIALE 🛑


            // 3. Procéder à la validation (maintenant que ApplicationUserId est renseigné)
            if (ModelState.IsValid)
            {
                // 4. Sauvegarder la fiche
                _context.Add(medecin);
                await _context.SaveChangesAsync();

                // Rediriger vers l'index
                return RedirectToAction(nameof(Index));
            }

            // Si la validation échoue, retourne la vue avec les erreurs.
            return View(medecin);
        }

        // GET: Medecin/Index (Simple liste pour vérifier la redirection)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var medecins = await _context.Medecins.ToListAsync();
            return View(medecins);
        }
    }
}