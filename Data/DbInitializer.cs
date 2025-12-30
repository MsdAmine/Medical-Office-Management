using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Création des Rôles
        string[] roleNames = { "Admin", "Medecin", "Personnel" };
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
        string adminRole = "Admin";

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
        else
        {
            // FORCE LA MISE À JOUR (Si l'utilisateur existe déjà)

            // On s'assure que l'email est confirmé
            adminUser.EmailConfirmed = true;
            await userManager.UpdateAsync(adminUser);

            // On réinitialise le mot de passe de force
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            await userManager.ResetPasswordAsync(adminUser, token, adminPassword);

            // On s'assure qu'il a le rôle Admin
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}