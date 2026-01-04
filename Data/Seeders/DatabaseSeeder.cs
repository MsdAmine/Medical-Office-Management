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

            if (!await context.BillingInvoices.AnyAsync())
            {
                context.BillingInvoices.AddRange(
                    new BillingInvoice
                    {
                        InvoiceNumber = "INV-1024",
                        PatientName = "Alice Martin",
                        Service = "Annual Checkup",
                        Amount = 180.00m,
                        Status = "Paid",
                        IssuedOn = DateTime.Today.AddDays(-10),
                        DueDate = DateTime.Today.AddDays(-2),
                        PaymentMethod = "Visa"
                    },
                    new BillingInvoice
                    {
                        InvoiceNumber = "INV-1025",
                        PatientName = "Marc Dupont",
                        Service = "Laboratory Tests",
                        Amount = 240.00m,
                        Status = "Pending",
                        IssuedOn = DateTime.Today.AddDays(-3),
                        DueDate = DateTime.Today.AddDays(12),
                        PaymentMethod = "Insurance"
                    },
                    new BillingInvoice
                    {
                        InvoiceNumber = "INV-1026",
                        PatientName = "Fatima Zahra",
                        Service = "Orthopedic Consultation",
                        Amount = 320.00m,
                        Status = "Overdue",
                        IssuedOn = DateTime.Today.AddDays(-20),
                        DueDate = DateTime.Today.AddDays(-5),
                        PaymentMethod = "Bank Transfer"
                    }
                );
            }

            if (!await context.InventoryItems.AnyAsync())
            {
                context.InventoryItems.AddRange(
                    new InventoryItem
                    {
                        ItemName = "Surgical Masks",
                        Category = "PPE",
                        Quantity = 450,
                        ReorderLevel = 300,
                        Status = "In Stock"
                    },
                    new InventoryItem
                    {
                        ItemName = "Nitrile Gloves",
                        Category = "PPE",
                        Quantity = 180,
                        ReorderLevel = 250,
                        Status = "Low Stock"
                    },
                    new InventoryItem
                    {
                        ItemName = "IV Catheters",
                        Category = "Supplies",
                        Quantity = 75,
                        ReorderLevel = 100,
                        Status = "Reorder"
                    }
                );
            }

            if (!await context.ReportArtifacts.AnyAsync())
            {
                context.ReportArtifacts.AddRange(
                    new ReportArtifact
                    {
                        Title = "Monthly Revenue",
                        Period = "Last 30 days",
                        Owner = "Finance",
                        Status = "Scheduled",
                        GeneratedOn = DateTime.Today.AddDays(-1)
                    },
                    new ReportArtifact
                    {
                        Title = "Patient Throughput",
                        Period = "This Week",
                        Owner = "Operations",
                        Status = "Ready",
                        GeneratedOn = DateTime.Today
                    },
                    new ReportArtifact
                    {
                        Title = "Medication Usage",
                        Period = "Quarter to Date",
                        Owner = "Pharmacy",
                        Status = "In Progress",
                        GeneratedOn = DateTime.Today.AddDays(-3)
                    }
                );
            }

            await context.SaveChangesAsync();
        }
    }
}
