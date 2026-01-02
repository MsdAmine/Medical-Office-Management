using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(SignInManager<ApplicationUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await SignOutIfNeededAsync();
        return ResolveRedirect(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Logout")]
    public async Task<IActionResult> LogoutPost(string? returnUrl = null)
    {
        await SignOutIfNeededAsync();
        return ResolveRedirect(returnUrl);
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

    private IActionResult ResolveRedirect(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        // Fallback to a friendly logged out page so the action works even before login is wired up.
        return RedirectToAction(nameof(LoggedOut));
    }
}