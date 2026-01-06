using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Data.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider services, MedicalOfficeContext context)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var hasIdentityData = await context.Roles.AnyAsync() || await context.Users.AnyAsync();

            if (!hasIdentityData)
            {
                await SeedRolesAsync(roleManager);
                await SeedUsersAsync(userManager);
            }

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

            var medecinUser = await userManager.FindByEmailAsync("medecin@medoffice.local");
            if (!await context.Medecins.AnyAsync() && medecinUser != null)
            {
                context.Medecins.Add(new Medecin
                {
                    ApplicationUserId = medecinUser.Id,
                    NomPrenom = "Dr. Claire Leclerc",
                    Specialite = "Médecine générale",
                    Adresse = "12 Rue de la Santé",
                    Telephone = "+33155501010",
                    Email = "medecin@medoffice.local"
                });
            }

            var patientUser = await userManager.FindByEmailAsync("patient@medoffice.local");
            if (!await context.Patients.AnyAsync())
            {
                context.Patients.Add(new Patient
                {
                    ApplicationUserId = patientUser?.Id,
                    Nom = "Martin",
                    Prenom = "Alice",
                    Telephone = "+33155500001",
                    Email = "patient@medoffice.local",
                    Adresse = "5 Avenue des Lilas",
                    Sexe = "F",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            await context.SaveChangesAsync();

            var medecinId = await context.Medecins.Select(m => m.Id).FirstOrDefaultAsync();
            var patientId = await context.Patients.Select(p => p.Id).FirstOrDefaultAsync();

            if (patientId != 0 && medecinId != 0)
            {
                if (!await context.Consultations.AnyAsync())
                {
                    context.Consultations.Add(new Consultation
                    {
                        PatientId = patientId,
                        MedecinId = medecinId,
                        DateConsult = DateTime.UtcNow.AddDays(-2),
                        Observations = "Routine check-up; vitals within normal limits.",
                        Diagnostics = "Healthy adult",
                        RendezVousId = null
                    });
                }

                if (!await context.Prescriptions.AnyAsync())
                {
                    context.Prescriptions.Add(new Prescription
                    {
                        PatientId = patientId,
                        MedecinId = medecinId,
                        Medication = "Amoxicillin",
                        Dosage = "500mg",
                        Frequency = "Twice daily",
                        Status = "Active",
                        IssuedOn = DateTime.UtcNow.AddDays(-2),
                        NextRefill = DateTime.UtcNow.AddDays(12),
                        RefillsRemaining = 2,
                        Notes = "Take with food"
                    });
                }

                if (!await context.LabResults.AnyAsync())
                {
                    context.LabResults.AddRange(
                        new LabResult
                        {
                            PatientId = patientId,
                            MedecinId = medecinId,
                            TestName = "CBC with Differential",
                            Priority = "Routine",
                            Status = "Completed",
                            CollectedOn = DateTime.UtcNow.AddHours(-4),
                            ResultValue = "Within normal limits"
                        },
                        new LabResult
                        {
                            PatientId = patientId,
                            MedecinId = medecinId,
                            TestName = "Troponin I",
                            Priority = "STAT",
                            Status = "Pending Review",
                            CollectedOn = DateTime.UtcNow.AddHours(-1),
                            ResultValue = "0.03 ng/mL",
                            Notes = "Escalate if symptoms worsen"
                        }
                    );
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            var roles = new[]
            {
                SystemRoles.Admin,
                SystemRoles.Secretaire,
                SystemRoles.Medecin,
                SystemRoles.Patient
            };

            foreach (var roleName in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
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

            await CreateUserAsync("admin@medoffice.local", "Admin123!", SystemRoles.Admin);
            await CreateUserAsync("secretaire@medoffice.local", "Secretaire123!", SystemRoles.Secretaire);
            await CreateUserAsync("medecin@medoffice.local", "Medecin123!", SystemRoles.Medecin);
            await CreateUserAsync("patient@medoffice.local", "Patient123!", SystemRoles.Patient);
        }
    }
}
