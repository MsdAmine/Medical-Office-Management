using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SettingsController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SettingsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string email, string phoneNumber, string officeName)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Mise à jour des champs
            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.OfficeName = officeName;

            // Si l'email change, il faut aussi mettre à jour le UserName
            if (user.Email != email)
            {
                user.Email = email;
                user.UserName = email;
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                // Rafraîchir les cookies de connexion pour refléter les changements
                await _signInManager.RefreshSignInAsync(user);
                TempData["StatusMessage"] = "Profile updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors) ModelState.AddModelError("", error.Description);
            return View("Index", user);
        }
    }
}