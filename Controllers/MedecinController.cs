using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalOfficeManagement.Controllers
{
    // Restricts this entire controller to Admins only
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
        // Change: Removed [AllowAnonymous] if you want the dashboard sidebar 
        // to only show doctors to authenticated staff/admins.
        public async Task<IActionResult> Index()
        {
            var medecins = await _context.Medecins.Include(m => m.ApplicationUser).ToListAsync();
            return View(medecins);
        }

        // GET: Medecin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var medecin = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
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
            if (ModelState.IsValid)
            {
                // Logic to link Medecin info to Identity User
                var user = new ApplicationUser 
                { 
                    UserName = medecin.Email, 
                    Email = medecin.Email, 
                    PhoneNumber = medecin.Telephone 
                };
                
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

        // GET: Medecin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var medecin = await _context.Medecins.Include(m => m.ApplicationUser).FirstOrDefaultAsync(m => m.Id == id);
            return medecin == null ? NotFound() : View(medecin);
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

                // 1. Remove Medecin first to satisfy FK constraints
                _context.Medecins.Remove(medecin);
                await _context.SaveChangesAsync();

                // 2. Remove the associated Identity User
                if (user != null)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (!result.Succeeded)
                    {
                        // Optional: Log errors if user deletion fails
                        TempData["Error"] = "Doctor record deleted, but login account could not be removed.";
                    }
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MedecinExists(int id) => _context.Medecins.Any(e => e.Id == id);
    }
}