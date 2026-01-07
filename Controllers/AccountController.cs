using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels;
using MedicalOfficeManagement.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicalOfficeManagement.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _emailSender = emailSender;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [AllowAnonymous]
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

    [AllowAnonymous]
    [HttpGet]
    public IActionResult LoggedOut()
    {
        return View();
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        var model = new ProfileViewModel
        {
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address
        };

        return View(model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Get the current user by ID from claims to ensure we're updating the correct user
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        // Validate email uniqueness if changed
        if (user.Email != model.Email)
        {
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null && existingUserByEmail.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.Email), "This email is already taken by another user.");
                return View(model);
            }
        }

        // Validate username uniqueness if changed
        if (user.UserName != model.UserName)
        {
            var existingUserByUsername = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByUsername != null && existingUserByUsername.Id != user.Id)
            {
                ModelState.AddModelError(nameof(model.UserName), "This username is already taken by another user.");
                return View(model);
            }
        }

        // Update user properties
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Address = model.Address;
        user.PhoneNumber = model.PhoneNumber;

        // Update email if changed
        if (user.Email != model.Email)
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
            if (!setEmailResult.Succeeded)
            {
                foreach (var error in setEmailResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        // Update username if changed
        if (user.UserName != model.UserName)
        {
            var setUserNameResult = await _userManager.SetUserNameAsync(user, model.UserName);
            if (!setUserNameResult.Succeeded)
            {
                foreach (var error in setUserNameResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        // Save changes - this updates the existing user, not creates a new one
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        model.StatusMessage = "Your profile has been updated successfully.";
        model.IsSuccess = true;
        return View(model);
    }

    [Authorize]
    [HttpGet]
    public IActionResult Help()
    {
        return View(new HelpViewModel());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Help(HelpViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        try
        {
            // Create email body with user information
            var phoneNumberHtml = !string.IsNullOrEmpty(user.PhoneNumber) 
                ? $"<strong>Phone:</strong> {user.PhoneNumber}<br>" 
                : string.Empty;
            
            var emailBody = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #2563eb; color: white; padding: 20px; border-radius: 5px 5px 0 0; }}
                        .content {{ background-color: #f8fafc; padding: 20px; border: 1px solid #e2e8f0; }}
                        .info {{ background-color: white; padding: 15px; margin: 15px 0; border-left: 4px solid #2563eb; }}
                        .message {{ background-color: white; padding: 15px; margin: 15px 0; border: 1px solid #e2e8f0; border-radius: 5px; }}
                        .footer {{ text-align: center; padding: 20px; color: #64748b; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class=""container"">
                        <div class=""header"">
                            <h2>Support Request from Patient Portal</h2>
                        </div>
                        <div class=""content"">
                            <div class=""info"">
                                <strong>From:</strong> {user.FirstName} {user.LastName}<br>
                                <strong>Email:</strong> {user.Email}<br>
                                <strong>Username:</strong> {user.UserName}<br>
                                {phoneNumberHtml}
                                <strong>Date:</strong> {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                            </div>
                            <div class=""message"">
                                <h3>Subject: {model.Subject}</h3>
                                <p>{model.Message.Replace("\n", "<br>")}</p>
                            </div>
                        </div>
                        <div class=""footer"">
                            <p>This is an automated message from the Medical Office Management System.</p>
                        </div>
                    </div>
                </body>
                </html>";

            // Send email to support manager
            await _emailSender.SendAsync(
                "massine000@gmail.com",
                $"Support Request: {model.Subject}",
                emailBody
            );

            model.StatusMessage = "Your support request has been sent successfully. We will get back to you soon.";
            model.IsSuccess = true;
            // Clear form
            model.Subject = string.Empty;
            model.Message = string.Empty;
        }
        catch (Exception ex)
        {
            model.StatusMessage = $"An error occurred while sending your message. Please try again later. Error: {ex.Message}";
            model.IsSuccess = false;
        }

        return View(model);
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
