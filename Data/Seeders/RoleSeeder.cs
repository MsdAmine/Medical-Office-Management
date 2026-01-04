using Microsoft.AspNetCore.Identity;
using MedicalOfficeManagement.Models.Security;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Data.Seeders
{
    public static class RoleSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
        {
            if (await roleManager.Roles.AnyAsync())
            {
                return;
            }

            string[] roles = { SystemRoles.Admin, SystemRoles.Secretaire, SystemRoles.Medecin, SystemRoles.Patient };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
