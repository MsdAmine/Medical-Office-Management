using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        if (await roleManager.Roles.AnyAsync() || await userManager.Users.AnyAsync())
        {
            return;
        }

        // 1. Création des Rôles
        string[] roleNames = { SystemRoles.Admin, SystemRoles.Medecin, SystemRoles.Secretaire, SystemRoles.Patient };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Paramètres de l'Admin
        string adminEmail = "admin@email.com";
        string adminPassword = "TestPassword1!";
        string adminRole = SystemRoles.Admin;

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            // CRÉATION NOUVEL UTILISATEUR
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Crucial pour éviter le rejet
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}
