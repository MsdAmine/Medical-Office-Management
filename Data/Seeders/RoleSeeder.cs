using MedicalOfficeManagement.ViewModels.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MedicalOfficeManagement.Data.Seeders
{
    public static class RoleSeeder
    {
        private static readonly string[] Roles =
        {
            nameof(AppRole.Admin),
            nameof(AppRole.Physician),
            nameof(AppRole.Nurse),
            nameof(AppRole.Billing),
            nameof(AppRole.Receptionist)
        };

        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in Roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
