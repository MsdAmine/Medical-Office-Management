using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection; // Assurez-vous d'avoir ce using
using System.Threading.Tasks;

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1. Définition et Création des Rôles
        string[] roleNames = { "Admin", "Medecin", "Personnel" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Création ou Vérification du compte Administrateur
        string adminEmail = "admin@email.com";
        string adminPassword = "TestPassword1!";
        string adminRole = "Admin";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        // Si l'utilisateur n'existe PAS, nous le créons.
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                // Attribue le rôle "Admin" à l'utilisateur
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
            else
            {
                // Si la création échoue, nous affichons les erreurs pour le débogage (dans la console ou logs)
                // Ex: dotnet run affichera ceci si vous utilisez ILogger.
                foreach (var error in result.Errors)
                {
                    System.Console.WriteLine($"Erreur de création d'Admin : {error.Description}");
                }
            }
        }
        else
        {
            // S'il existe, s'assurer qu'il a le rôle Admin au cas où ce serait un ancien utilisateur sans rôle
            if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }
}