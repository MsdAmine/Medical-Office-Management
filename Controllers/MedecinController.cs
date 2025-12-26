using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using MedicalOfficeManagement.Data;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = "Admin")] // Seuls les Admins accèdent à cette gestion
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
                "Generalist", "Cardiologist", "Dermatologist", 
                "Pediatrician", "Neurologist", "Radiologist"
            };
            ViewBag.Specialites = new SelectList(specialites);
        }

        // GET: Medecin
        public async Task<IActionResult> Index()
        {
            var medecins = await _context.Medecins.Include(m => m.ApplicationUser).ToListAsync();
            return View(medecins);
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
            // Retirer les propriétés de navigation pour éviter les erreurs de validation
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)
            {
                // 1. Découpage du NomPrenom pour Identity
                string firstName = "";
                string lastName = "";

                if (!string.IsNullOrWhiteSpace(medecin.NomPrenom))
                {
                    var parts = medecin.NomPrenom.Trim().Split(' ');
                    firstName = parts[0];
                    lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
                }

                // 2. Création du compte utilisateur (Identity)
                var user = new ApplicationUser
                {
                    UserName = medecin.Email,
                    Email = medecin.Email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = medecin.Telephone,
                    EmailConfirmed = true
                };

                // CRÉATION AVEC LE MOT DE PASSE PAR DÉFAUT : Welcome@2025!
                var result = await _userManager.CreateAsync(user, "Welcome@2025!"); 

                if (result.Succeeded)
                {
                    // 3. Attribution du rôle EXACT présent en base
                    // Note: Changé "Doctor" par "Medecin" pour éviter l'erreur
                    await _userManager.AddToRoleAsync(user, "Medecin");

                    // 4. Liaison du médecin à l'utilisateur créé
                    medecin.ApplicationUserId = user.Id;
                    _context.Add(medecin);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            PopulateSpecialitesViewBag();
            return View(medecin);
        }

        // GET: Medecin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var medecin = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            if (medecin == null) return NotFound();
            PopulateSpecialitesViewBag();
            return View(medecin);
        }

        // POST: Medecin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Medecin model)
        {
            if (id != model.Id) return NotFound();

            // Retirer les propriétés liées à Identity de la validation du modèle Medecin
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");

            if (ModelState.IsValid)
            {
                var medecinToUpdate = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
                if (medecinToUpdate == null) return NotFound();

                // Mise à jour des infos de la table Medecin
                medecinToUpdate.NomPrenom = model.NomPrenom;
                medecinToUpdate.Specialite = model.Specialite;
                medecinToUpdate.Adresse = model.Adresse;
                medecinToUpdate.Telephone = model.Telephone;
                medecinToUpdate.Email = model.Email;

                // Mise à jour synchronisée des infos Identity
                if (medecinToUpdate.ApplicationUser != null)
                {
                    var user = medecinToUpdate.ApplicationUser;
                    
                    if (!string.IsNullOrWhiteSpace(model.NomPrenom))
                    {
                        var parts = model.NomPrenom.Trim().Split(' ');
                        user.FirstName = parts[0];
                        user.LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
                    }

                    user.Email = model.Email;
                    user.UserName = model.Email;
                    user.PhoneNumber = model.Telephone;
                    await _userManager.UpdateAsync(user);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateSpecialitesViewBag();
            return View(model);
        }

        // GET: Medecin/Delete/5
        [HttpGet]
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
            var medecin = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            if (medecin != null)
            {
                var user = medecin.ApplicationUser;
                _context.Medecins.Remove(medecin);
                await _context.SaveChangesAsync();

                // On supprime aussi le compte utilisateur associé
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}