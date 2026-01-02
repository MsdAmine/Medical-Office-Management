using MedicalOfficeManagement.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Data.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var scopedProvider = scope.ServiceProvider;

            var context = scopedProvider.GetRequiredService<MedicalOfficeContext>();

            await context.Database.MigrateAsync();

            var roleManager = scopedProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scopedProvider.GetRequiredService<UserManager<ApplicationUser>>();

            var roles = new[] { "Admin", "Secretaire", "Medecin", "Patient" };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            async Task CreateUserAsync(string email, string password, string role)
            {
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, password);

                    if (!result.Succeeded)
                    {
                        return;
                    }
                }
                else if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                }

                if (!await userManager.IsInRoleAsync(user, role))
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }

            await CreateUserAsync("admin@medoffice.local", "Admin123!", "Admin");
            await CreateUserAsync("secretaire@medoffice.local", "Secretaire123!", "Secretaire");
            await CreateUserAsync("medecin@medoffice.local", "Medecin123!", "Medecin");
            await CreateUserAsync("patient@medoffice.local", "Patient123!", "Patient");
        }
    }
}
