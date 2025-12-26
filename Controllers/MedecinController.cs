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
                var user = new ApplicationUser 
                { 
                    UserName = medecin.Email, 
                    Email = medecin.Email, 
                    PhoneNumber = medecin.Telephone 
                };
                
                // Note: Mot de passe par défaut pour le docteur
                var result = await _userManager.CreateAsync(user, "Doctor@123");

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync("Medecin"))
                        await _roleManager.CreateAsync(new IdentityRole("Medecin"));

                    await _userManager.AddToRoleAsync(user, "Medecin");
                    
                    medecin.ApplicationUserId = user.Id;
                    _context.Medecins.Add(medecin);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            }
            PopulateSpecialitesViewBag();
            return View(medecin);
        }

        // ... Gardez vos méthodes Edit, Details et Delete telles quelles ...
    }
}