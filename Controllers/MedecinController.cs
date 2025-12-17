using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MedecinController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public MedecinController(
            MedicalOfficeContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private void PopulateSpecialitesViewBag()
        {
            var specialites = new List<string>
            {
                "Cardiologie", "Dermatologie", "Généraliste",
                "Ophtalmologie", "Pédiatrie", "Radiologie", "Urologie"
            };
            ViewBag.Specialites = new SelectList(specialites);
        }

        // GET: Medecin
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var medecins = await _context.Medecins
                .Include(m => m.ApplicationUser)
                .ToListAsync();
            return View(medecins);
        }

        // GET: Medecin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var medecin = await _context.Medecins
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            return medecin == null ? NotFound() : View(medecin);
        }

        // GET: Medecin/Create
        public IActionResult Create()
        {
            PopulateSpecialitesViewBag();
            return View();
        }

        // POST: Medecin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Medecin medecin)
        {
            if (!ModelState.IsValid)
            {
                PopulateSpecialitesViewBag();
                return View(medecin);
            }

            // Check if email/username already exists
            var existingUser = await _userManager.FindByNameAsync(medecin.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé par un autre compte.");
                PopulateSpecialitesViewBag();
                return View(medecin);
            }

            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = medecin.Email,
                Email = medecin.Email,
                PhoneNumber = medecin.Telephone
            };

            // Utilisation d'un mot de passe temporaire pour la création
            var result = await _userManager.CreateAsync(user, "Doctor@123");
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                PopulateSpecialitesViewBag();
                return View(medecin);
            }

            // Ensure Medecin role exists
            if (!await _roleManager.RoleExistsAsync("Medecin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Medecin"));
            }

            // Assign role
            await _userManager.AddToRoleAsync(user, "Medecin");

            // Link doctor to Identity user
            medecin.ApplicationUserId = user.Id;

            // Save Medecin
            _context.Medecins.Add(medecin);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Medecin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            // Inclure l'utilisateur Identity pour afficher l'Email/Telephone actuel
            var medecin = await _context.Medecins
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medecin == null) return NotFound();

            PopulateSpecialitesViewBag();
            return View(medecin);
        }

        // POST: Medecin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Utilisation du modèle lié pour une liaison facile des données
        public async Task<IActionResult> Edit(int id, Medecin model)
        {
            if (id != model.Id) return NotFound();

            // 1. Charger l'objet Medecin AVEC l'ApplicationUser
            var medecinToUpdate = await _context.Medecins
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medecinToUpdate == null) return NotFound();

            // Si ModelState est invalide (malgré [ValidateNever] ou [Required] manquants), retourner la vue
            if (!ModelState.IsValid)
            {
                PopulateSpecialitesViewBag();
                // Assurez-vous que l'objet ApplicationUser n'est pas perdu dans la vue rechargée
                model.ApplicationUser = medecinToUpdate.ApplicationUser;
                return View(model);
            }

            // 2. Mettre à jour les champs de l'entité Medecin
            medecinToUpdate.NomPrenom = model.NomPrenom;
            medecinToUpdate.Specialite = model.Specialite;
            medecinToUpdate.Adresse = model.Adresse;
            // MedecinToUpdate contient maintenant aussi les nouveaux Email/Telephone du formulaire
            medecinToUpdate.Telephone = model.Telephone;
            medecinToUpdate.Email = model.Email;

            // 3. Mettre à jour l'utilisateur Identity (Email/Telephone/Username)
            if (medecinToUpdate.ApplicationUser != null)
            {
                var user = medecinToUpdate.ApplicationUser;
                IdentityResult result = IdentityResult.Success; // Initialisation pour le premier test

                // Mettre à jour l'Email et le UserName dans Identity
                if (user.Email != model.Email)
                {
                    var emailToken = await _userManager.GenerateChangeEmailTokenAsync(user, model.Email);
                    result = await _userManager.ChangeEmailAsync(user, model.Email, emailToken);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors) ModelState.AddModelError("", $"Email: {error.Description}");
                    }
                    else
                    {
                        // Si l'email change, le Username doit souvent changer aussi
                        await _userManager.SetUserNameAsync(user, model.Email);
                    }
                }

                // Mettre à jour le Téléphone dans Identity
                if (result.Succeeded && user.PhoneNumber != model.Telephone)
                {
                    var phoneToken = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.Telephone);
                    result = await _userManager.ChangePhoneNumberAsync(user, model.Telephone, phoneToken);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors) ModelState.AddModelError("", $"Téléphone: {error.Description}");
                    }
                }

                // Si la mise à jour Identity a échoué, retourner la vue avec les erreurs
                if (!result.Succeeded || !ModelState.IsValid)
                {
                    PopulateSpecialitesViewBag();
                    return View(medecinToUpdate);
                }
            }

            // 4. Sauvegarder Medecin (les changements de l'ApplicationUser sont sauvés par le userManager)
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Medecins.Any(e => e.Id == id)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Medecin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var medecin = await _context.Medecins
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medecin == null) return NotFound();

            return View(medecin);
        }

        // POST: Medecin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var medecin = await _context.Medecins
                .Include(m => m.ApplicationUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (medecin == null) return NotFound();

            // 🛑 CORRECTION CLÉ : Supprimer UNIQUEMENT l'ApplicationUser.
            // La suppression de l'entité Medecin sera gérée par la cascade de la DB.
            if (medecin.ApplicationUser != null)
            {
                var result = await _userManager.DeleteAsync(medecin.ApplicationUser);

                if (!result.Succeeded)
                {
                    // Si la suppression Identity échoue, enregistrer l'erreur et rediriger vers la suppression.
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", $"Erreur de suppression Identity: {error.Description}");
                    }
                    // Le modèle peut être nul ici, soyez prudent avec ce que vous passez à la vue.
                    return RedirectToAction(nameof(Delete), new { id = id });
                }
            }

            // Si la suppression en cascade est désactivée dans votre DB, vous auriez besoin de supprimer Medecin ici:
            // _context.Medecins.Remove(medecin);
            // await _context.SaveChangesAsync();

            // Dans le cas de Cascade Delete (standard avec Identity), le Medecin a été supprimé.
            return RedirectToAction(nameof(Index));
        }

        private bool MedecinExists(int id)
        {
            return _context.Medecins.Any(e => e.Id == id);
        }
    }
}