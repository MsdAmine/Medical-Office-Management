using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using MedicalOfficeManagement.Data;

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
                "Generalist", "Cardiologist", "Dermatologist",
                "Pediatrician", "Neurologist", "Radiologist"
            };
            ViewBag.Specialites = new SelectList(specialites);
        }

        // --- INDEX : Affiche tout le staff (Médecins et Secrétaires) ---
        public async Task<IActionResult> Index()
        {
            var allStaff = new List<StaffVM>();
            var users = await _userManager.Users.ToListAsync();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                if (role == "Medecin" || role == "Personnel")
                {
                    var medecinData = await _context.Medecins.FirstOrDefaultAsync(m => m.ApplicationUserId == user.Id);

                    allStaff.Add(new StaffVM
                    {
                        UserId = user.Id,
                        NomPrenom = medecinData?.NomPrenom ?? $"{user.FirstName} {user.LastName}",
                        Email = user.Email,
                        Telephone = user.PhoneNumber,
                        Role = role,
                        Specialite = medecinData?.Specialite ?? "N/A",
                        Adresse = medecinData?.Adresse ?? "Administrative Office",
                        MedecinId = medecinData?.Id
                    });
                }
            }
            return View(allStaff);
        }

        // --- CREATE : Création unifiée ---
        public IActionResult Create()
        {
            PopulateSpecialitesViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Medecin medecin, string SelectedRole, string TelephonePersonnel, string AdressePersonnel)
        {
            // On ignore la validation du modèle Medecin si c'est du personnel
            if (SelectedRole == "Personnel")
            {
                ModelState.Clear(); // On vide les erreurs car on ne va pas sauvegarder un "Medecin"
            }
            else
            {
                ModelState.Remove("ApplicationUser");
                ModelState.Remove("ApplicationUserId");
            }

            if (SelectedRole == "Personnel" || ModelState.IsValid)
            {
                // Extraction du nom/prénom
                var parts = medecin.NomPrenom?.Trim().Split(' ') ?? new string[] { "Staff", "Member" };

                var user = new ApplicationUser
                {
                    UserName = medecin.Email,
                    Email = medecin.Email,
                    FirstName = parts[0],
                    LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "",
                    // On utilise les nouveaux champs passés en paramètres pour le personnel
                    PhoneNumber = SelectedRole == "Personnel" ? TelephonePersonnel : medecin.Telephone,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, "Welcome@2025!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, SelectedRole);

                    if (SelectedRole == "Medecin")
                    {
                        medecin.ApplicationUserId = user.Id;
                        _context.Add(medecin);
                        await _context.SaveChangesAsync();
                    }
                    // Pour le personnel, on ne fait rien en base SQL : tout est dans Identity

                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }

            PopulateSpecialitesViewBag();
            return View(medecin);
        }

        // --- EDIT MÉDECIN (Profil complet) ---
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
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");

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
                    var parts = model.NomPrenom?.Trim().Split(' ');
                    user.FirstName = parts?[0] ?? "";
                    user.LastName = parts?.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
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

        // --- EDIT STAFF (Personnel uniquement) ---
        // --- EDIT STAFF (Personnel uniquement) ---
        public async Task<IActionResult> EditStaff(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // On crée un modèle temporaire pour la vue
            var model = new Medecin
            {
                ApplicationUserId = user.Id,
                Email = user.Email,
                Telephone = user.PhoneNumber,
                NomPrenom = $"{user.FirstName} {user.LastName}",
                // On peut utiliser Adresse pour stocker une info supplémentaire dans Identity si besoin, 
                // ou simplement l'afficher comme "Administrative"
                Adresse = "Administrative Office"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(string id, Medecin model, string ServicePersonnel)
        {
            var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
            if (user == null) return NotFound();

            // CRUCIAL : On ignore les validations du modèle Medecin (SQL)
            ModelState.Remove("Id");
            ModelState.Remove("Specialite");
            ModelState.Remove("ApplicationUser");

            if (ModelState.IsValid)
            {
                var parts = model.NomPrenom?.Trim().Split(' ');
                user.FirstName = parts?[0] ?? "";
                user.LastName = parts?.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
                user.Email = model.Email;
                user.UserName = model.Email;
                user.PhoneNumber = model.Telephone;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // --- DELETE UNIFIÉ (Réutilise une seule vue Delete.cshtml) ---
        public async Task<IActionResult> Delete(int? id, string userId)
        {
            Medecin model;

            if (id.HasValue && id > 0)
            {
                model = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null) return NotFound();
                model = new Medecin
                {
                    ApplicationUserId = user.Id,
                    NomPrenom = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Specialite = "Administrative Staff",
                    ApplicationUser = user
                };
            }
            else return NotFound();

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id, string ApplicationUserId)
        {
            if (id.HasValue && id > 0)
            {
                var medecin = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
                if (medecin != null)
                {
                    var user = medecin.ApplicationUser;
                    _context.Medecins.Remove(medecin);
                    await _context.SaveChangesAsync();
                    if (user != null) await _userManager.DeleteAsync(user);
                }
            }
            else if (!string.IsNullOrEmpty(ApplicationUserId))
            {
                var user = await _userManager.FindByIdAsync(ApplicationUserId);
                if (user != null) await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }
    }

    public class StaffVM
    {
        public string UserId { get; set; }
        public int? MedecinId { get; set; }
        public string NomPrenom { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Specialite { get; set; }
        public string Role { get; set; }
        public string Adresse { get; set; }
    }
}