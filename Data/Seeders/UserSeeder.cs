using System;
using System.Linq;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalOfficeManagement.Data.Seeders
{
    public static class UserSeeder
    {
        private const string DefaultPassword = "P@ssw0rd!";

        private static readonly (string Email, string FirstName, string LastName, AppRole Role)[] SeedUsers =
        {
            ("admin@email.com", "System", "Admin", AppRole.Admin),
            ("doctor@email.com", "Courtney", "Physician", AppRole.Physician),
            ("nurse@email.com", "Alex", "Nurse", AppRole.Nurse),
            ("billing@email.com", "Bailey", "Billing", AppRole.Billing),
            ("reception@email.com", "Riley", "Reception", AppRole.Receptionist)
        };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var (email, firstName, lastName, role) in SeedUsers)
            {
                var roleName = role.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }

                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(user, DefaultPassword);
                    if (!createResult.Succeeded)
                    {
                        continue;
                    }
                }
                else if (!user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    await userManager.UpdateAsync(user);
                }

                var userRoles = await userManager.GetRolesAsync(user);
                foreach (var existingRole in userRoles.Where(r => !string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase)))
                {
                    await userManager.RemoveFromRoleAsync(user, existingRole);
                }

                if (!await userManager.IsInRoleAsync(user, roleName))
                {
                    await userManager.AddToRoleAsync(user, roleName);
                }
            }
        }
    }
}
