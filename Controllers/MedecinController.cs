using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using MedicalOfficeManagement.Data;

namespace MedicalOfficeManagement.Controllers
{
    public class StaffVM
    {
        public string UserId { get; set; }
        public int? MedecinId { get; set; }
        public string NomPrenom { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Specialite { get; set; }
        public string Role { get; set; }
        public string Address { get; set; }
    }

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

        private void PopulateSpecialitesViewBag()
        {
            var specs = new List<string> { "Generalist", "Cardiologist", "Dermatologist", "Pediatrician", "Neurologist" };
            ViewBag.Specialites = new SelectList(specs);
        }

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
                        Specialite = medecinData?.Specialite ?? "Administrative",
                        Address = user.Address ?? "N/A",
                        MedecinId = medecinData?.Id
                    });
                }
            }
            return View(allStaff);
        }

        public IActionResult Create()
        {
            PopulateSpecialitesViewBag();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Medecin medecin, string SelectedRole, string TelephonePersonnel, string ServicePersonnel)
        {
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");
            ModelState.Remove("Id");

            if (SelectedRole == "Medecin")
            {
                ModelState.Remove("TelephonePersonnel");
                ModelState.Remove("ServicePersonnel");
                if (string.IsNullOrWhiteSpace(medecin.Adresse)) medecin.Adresse = "Cabinet Medical";
            }
            else
            {
                ModelState.Remove("Specialite");
                ModelState.Remove("Telephone");
                ModelState.Remove("Adresse");
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = medecin.Email,
                    Email = medecin.Email,
                    FirstName = medecin.NomPrenom.Split(' ')[0],
                    LastName = medecin.NomPrenom.Contains(" ") ? medecin.NomPrenom.Substring(medecin.NomPrenom.IndexOf(' ') + 1) : "Staff",
                    PhoneNumber = SelectedRole == "Personnel" ? TelephonePersonnel : medecin.Telephone,
                    Address = medecin.Adresse,
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
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
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
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("ApplicationUserId");
            if (ModelState.IsValid)
            {
                var medecin = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
                if (medecin != null)
                {
                    medecin.NomPrenom = model.NomPrenom;
                    medecin.Email = model.Email;
                    medecin.Telephone = model.Telephone;
                    medecin.Specialite = model.Specialite;
                    medecin.Adresse = model.Adresse;

                    if (medecin.ApplicationUser != null)
                    {
                        medecin.ApplicationUser.Address = model.Adresse;
                        await _userManager.UpdateAsync(medecin.ApplicationUser);
                    }

                    _context.Update(medecin);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            PopulateSpecialitesViewBag();
            return View(model);
        }

        public async Task<IActionResult> EditStaff(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new Medecin
            {
                ApplicationUserId = user.Id,
                Email = user.Email,
                Telephone = user.PhoneNumber,
                NomPrenom = $"{user.FirstName} {user.LastName}",
                Adresse = user.Address
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(Medecin model)
        {
            ModelState.Remove("Id");
            ModelState.Remove("Specialite");
            ModelState.Remove("ApplicationUser");
            ModelState.Remove("Adresse");

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.ApplicationUserId);
                if (user != null)
                {
                    var parts = model.NomPrenom.Split(' ');
                    user.FirstName = parts[0];
                    user.LastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";
                    user.Email = model.Email;
                    user.PhoneNumber = model.Telephone;
                    user.Address = model.Adresse;
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id, string userId)
        {
            Medecin model = null;
            if (id.HasValue && id > 0)
                model = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            else if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null) model = new Medecin { ApplicationUserId = user.Id, NomPrenom = $"{user.FirstName} {user.LastName}", Email = user.Email, Specialite = "Administrative" };
            }

            return model == null ? NotFound() : View(model);
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
}