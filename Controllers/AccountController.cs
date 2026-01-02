using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await ResolveUserAsync(model.UsernameOrEmail);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return RedirectToLocal(model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked out.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await SignOutIfNeededAsync();
        return RedirectToAction(nameof(Login), new { returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Logout")]
    public async Task<IActionResult> LogoutPost(string? returnUrl = null)
    {
        await SignOutIfNeededAsync();
        return RedirectToAction(nameof(Login), new { returnUrl });
    }

    [HttpGet]
    public IActionResult LoggedOut()
    {
        return View();
    }

    private async Task SignOutIfNeededAsync()
    {
        if (_signInManager.IsSignedIn(User))
        {
            await _signInManager.SignOutAsync();
        }
    }

    private async Task<ApplicationUser?> ResolveUserAsync(string usernameOrEmail)
    {
        var user = await _userManager.FindByNameAsync(usernameOrEmail);

        if (user == null && usernameOrEmail.Contains("@", StringComparison.Ordinal))
        {
            user = await _userManager.FindByEmailAsync(usernameOrEmail);
        }

        return user;
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
