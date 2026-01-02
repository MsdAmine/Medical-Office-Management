using Microsoft.AspNetCore.Identity;
using MedicalOfficeManagement.Models;

namespace MedicalOfficeManagement.Data.Seeders
{
    public static class UserSeeder
    {
        public static async Task SeedAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            var adminEmail = "admin@clinic.local";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, "Admin123!");
                await userManager.AddToRoleAsync(admin, "admin");
            }
        }
    }
}
