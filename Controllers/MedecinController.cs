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
                "Cardiologie", "Dermatologie", "Généraliste",
                "Ophtalmologie", "Pédiatrie", "Radiologie", "Urologie"
            };
            ViewBag.Specialites = new SelectList(specialites);
        }

        public async Task<IActionResult> Index()
        {
            var medecins = await _context.Medecins.Include(m => m.ApplicationUser).ToListAsync();
            return View(medecins);
        }

        public IActionResult Create()
        {
            PopulateSpecialitesViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Medecin medecin)
        {
            if (ModelState.IsValid)
            {
                // Création du compte utilisateur pour la connexion
                var user = new ApplicationUser 
                { 
                    UserName = medecin.Email, 
                    Email = medecin.Email, 
                    PhoneNumber = medecin.Telephone 
                };
                
                // Mot de passe par défaut
                var result = await _userManager.CreateAsync(user, "Doctor@123");

                if (result.Succeeded)
                {
                    // Vérification et création du rôle Medecin si nécessaire
                    if (!await _roleManager.RoleExistsAsync("Medecin"))
                        await _roleManager.CreateAsync(new IdentityRole("Medecin"));

                    await _userManager.AddToRoleAsync(user, "Medecin");
                    
                    // Liaison entre le profil Medecin et l'utilisateur Identity
                    medecin.ApplicationUserId = user.Id;
                    _context.Medecins.Add(medecin);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                
                // Si la création de l'utilisateur échoue (ex: email déjà pris)
                foreach (var error in result.Errors) 
                    ModelState.AddModelError("", error.Description);
            }
            PopulateSpecialitesViewBag();
            return View(medecin);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var medecin = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            if (medecin == null) return NotFound();
            PopulateSpecialitesViewBag();
            return View(medecin);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Medecin model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var medecinToUpdate = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
                if (medecinToUpdate == null) return NotFound();

                medecinToUpdate.NomPrenom = model.NomPrenom;
                medecinToUpdate.Specialite = model.Specialite;
                medecinToUpdate.Adresse = model.Adresse;
                medecinToUpdate.Telephone = model.Telephone;
                medecinToUpdate.Email = model.Email;

                if (medecinToUpdate.ApplicationUser != null)
                {
                    var user = medecinToUpdate.ApplicationUser;
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

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}