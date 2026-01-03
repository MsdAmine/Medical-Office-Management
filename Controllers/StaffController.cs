using System.Linq;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers;

[Authorize(Roles = SystemRoles.Admin)]
public class StaffController : Controller
{
    private readonly MedicalOfficeContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffController(MedicalOfficeContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? statusMessage = null)
    {
        SetPageMetadata();

        var viewModel = new StaffIndexViewModel
        {
            StatusMessage = statusMessage,
            StaffForm = new StaffCreateViewModel(),
            Medecins = await GetMedecinsAsync(),
            Secretaires = await GetSecretairesAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(StaffIndexViewModel viewModel)
    {
        SetPageMetadata();

        viewModel ??= new StaffIndexViewModel { StaffForm = new StaffCreateViewModel() };

        if (viewModel?.StaffForm == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid submission.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateListsAsync(viewModel);
            return View(viewModel);
        }

        var form = viewModel.StaffForm;

        if (!IsSupportedRole(form.Role))
        {
            ModelState.AddModelError("StaffForm.Role", "Please select a valid role.");
            await PopulateListsAsync(viewModel);
            return View(viewModel);
        }

        if (form.Role == SystemRoles.Medecin)
        {
            ValidateMedecinFields(form);
        }

        if (!ModelState.IsValid)
        {
            await PopulateListsAsync(viewModel);
            return View(viewModel);
        }

        var user = new ApplicationUser
        {
            UserName = form.Email,
            Email = form.Email,
            FirstName = form.FirstName,
            LastName = form.LastName,
            PhoneNumber = form.PhoneNumber,
            Address = form.Role == SystemRoles.Medecin ? form.MedecinAdresse ?? form.Address : form.Address
        };

        var creationResult = await _userManager.CreateAsync(user, form.Password);
        if (!creationResult.Succeeded)
        {
            foreach (var error in creationResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            await PopulateListsAsync(viewModel);
            return View(viewModel);
        }

        await _userManager.AddToRoleAsync(user, form.Role);

        if (form.Role == SystemRoles.Medecin)
        {
            var medecin = new Medecin
            {
                NomPrenom = BuildFullName(form.FirstName, form.LastName),
                Specialite = form.Specialite!,
                Adresse = form.MedecinAdresse ?? form.Address ?? string.Empty,
                Telephone = form.MedecinTelephone ?? form.PhoneNumber ?? string.Empty,
                Email = form.Email,
                ApplicationUserId = user.Id
            };

            _context.Medecins.Add(medecin);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index), new { statusMessage = "Staff account created successfully." });
    }

    private static bool IsSupportedRole(string role)
    {
        return string.Equals(role, SystemRoles.Medecin, StringComparison.OrdinalIgnoreCase)
               || string.Equals(role, SystemRoles.Secretaire, StringComparison.OrdinalIgnoreCase);
    }

    private void ValidateMedecinFields(StaffCreateViewModel form)
    {
        if (string.IsNullOrWhiteSpace(form.Specialite))
        {
            ModelState.AddModelError("StaffForm.Specialite", "Specialty is required for médecins.");
        }

        if (string.IsNullOrWhiteSpace(form.MedecinAdresse) && string.IsNullOrWhiteSpace(form.Address))
        {
            ModelState.AddModelError("StaffForm.MedecinAdresse", "An office address is required for médecins.");
        }

        if (string.IsNullOrWhiteSpace(form.MedecinTelephone) && string.IsNullOrWhiteSpace(form.PhoneNumber))
        {
            ModelState.AddModelError("StaffForm.MedecinTelephone", "A phone number is required for médecins.");
        }
    }

    private async Task PopulateListsAsync(StaffIndexViewModel viewModel)
    {
        viewModel.Medecins = await GetMedecinsAsync();
        viewModel.Secretaires = await GetSecretairesAsync();
    }

    private async Task<IEnumerable<StaffListItemViewModel>> GetSecretairesAsync()
    {
        var users = await _userManager.GetUsersInRoleAsync(SystemRoles.Secretaire);

        return users
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new StaffListItemViewModel
            {
                FullName = BuildFullName(u.FirstName, u.LastName, u.Email),
                Email = u.Email ?? string.Empty,
                RoleLabel = "Secrétaire",
                Detail = string.IsNullOrWhiteSpace(u.PhoneNumber) ? null : u.PhoneNumber
            });
    }

    private async Task<IEnumerable<StaffListItemViewModel>> GetMedecinsAsync()
    {
        var medecins = await _context.Medecins
            .Include(m => m.ApplicationUser)
            .OrderBy(m => m.NomPrenom)
            .ToListAsync();

        return medecins.Select(m => new StaffListItemViewModel
        {
            FullName = string.IsNullOrWhiteSpace(m.NomPrenom)
                ? BuildFullName(m.ApplicationUser?.FirstName, m.ApplicationUser?.LastName, m.Email)
                : m.NomPrenom,
            Email = m.Email,
            RoleLabel = "Médecin",
            Detail = m.Specialite
        });
    }

    private static string BuildFullName(string? firstName, string? lastName, string? fallback = null)
    {
        var parts = new[] { firstName, lastName }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        if (parts.Length == 0)
        {
            return fallback ?? "Unlisted";
        }

        return string.Join(" ", parts);
    }

    private void SetPageMetadata()
    {
        ViewData["Title"] = "Staff Management";
        ViewData["Breadcrumb"] = "Administration";
    }
}
