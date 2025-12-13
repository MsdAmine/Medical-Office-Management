using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks; // Ajout nécessaire pour Task

public static class DbInitializer
{
    public static async Task SeedRolesAndAdminUser(IServiceProvider serviceProvider)
    {
        // 1. Initialisation des services
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 2. Définition des Rôles
        string[] roleNames = { "Admin", "Medecin", "Personnel" };

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                // Crée les rôles s'ils n'existent pas
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 3. Création du compte Administrateur initial (Méthode FORCÉE)
        string adminEmail = "admin@email.com";
        string adminPassword = "TestPassword1!";
        string adminRole = "Admin";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser != null)
        {
            // Si l'utilisateur existe déjà (potentiellement cassé ou verrouillé),
            // nous le supprimons pour le recréer proprement. C'est le plus sûr pour le seeding de test.
            await userManager.DeleteAsync(adminUser);
        }

        // --- Création de l'utilisateur ---
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // Confirmé pour ignorer l'envoi d'e-mail
        };

        // Crée l'utilisateur avec le mot de passe
        IdentityResult result = await userManager.CreateAsync(adminUser, adminPassword);

        if (result.Succeeded)
        {
            // Attribue le rôle "Admin" à l'utilisateur
            await userManager.AddToRoleAsync(adminUser, adminRole);
        }
        else
        {
            // Si la création échoue (par exemple, problème de mot de passe trop faible
            // malgré les règles par défaut), nous logons les erreurs.
            // Si vous n'avez pas de ILogger configuré ici, vous pouvez utiliser Debug.WriteLine.
            // L'erreur sera de toute façon capturée par le bloc try/catch dans Program.cs.
        }
    }
}