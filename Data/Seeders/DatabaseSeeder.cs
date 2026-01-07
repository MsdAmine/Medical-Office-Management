using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Data.Seeders
{
    public static class DatabaseSeeder
    {
        private const string DefaultPassword = "massine123!";

        public static async Task SeedAsync(IServiceProvider services, MedicalOfficeContext context)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Seed roles first
            await SeedRolesAsync(roleManager);

            // Seed users
            var users = await SeedUsersAsync(userManager, context);

            // Seed doctors
            var medecins = await SeedMedecinsAsync(context, users);

            // Seed patients
            var patients = await SeedPatientsAsync(context, users);

            // Seed appointments
            var appointments = await SeedAppointmentsAsync(context, patients, medecins);

            // Seed consultations
            await SeedConsultationsAsync(context, patients, medecins, appointments);

            // Seed prescriptions
            await SeedPrescriptionsAsync(context, patients, medecins);

            // Seed lab results
            await SeedLabResultsAsync(context, patients, medecins);

            // Seed billing invoices
            await SeedBillingInvoicesAsync(context, patients);

            // Seed inventory items
            await SeedInventoryItemsAsync(context);

            // Seed report artifacts
            await SeedReportArtifactsAsync(context);

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

        private static async Task<Dictionary<string, ApplicationUser>> SeedUsersAsync(
            UserManager<ApplicationUser> userManager, 
            MedicalOfficeContext context)
        {
            var users = new Dictionary<string, ApplicationUser>();

            async Task<ApplicationUser> CreateUserAsync(string email, string firstName, string lastName, string role)
            {
                var user = await userManager.FindByEmailAsync(email);

                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true,
                        FirstName = firstName,
                        LastName = lastName,
                        PhoneNumber = $"+3315550{new Random().Next(1000, 9999)}"
                    };

                    var result = await userManager.CreateAsync(user, DefaultPassword);

                    if (!result.Succeeded)
                    {
                        return null!;
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

                return user;
            }

            // Admin
            users["admin"] = await CreateUserAsync("admin@medoffice.local", "Admin", "System", SystemRoles.Admin);

            // Secretaries
            users["secretaire1"] = await CreateUserAsync("marie.dupont@medoffice.local", "Marie", "Dupont", SystemRoles.Secretaire);
            users["secretaire2"] = await CreateUserAsync("sophie.martin@medoffice.local", "Sophie", "Martin", SystemRoles.Secretaire);

            // Doctors
            users["medecin1"] = await CreateUserAsync("claire.leclerc@medoffice.local", "Claire", "Leclerc", SystemRoles.Medecin);
            users["medecin2"] = await CreateUserAsync("pierre.dubois@medoffice.local", "Pierre", "Dubois", SystemRoles.Medecin);
            users["medecin3"] = await CreateUserAsync("anne.bernard@medoffice.local", "Anne", "Bernard", SystemRoles.Medecin);
            users["medecin4"] = await CreateUserAsync("thomas.moreau@medoffice.local", "Thomas", "Moreau", SystemRoles.Medecin);

            // Patients
            users["patient1"] = await CreateUserAsync("alice.martin@medoffice.local", "Alice", "Martin", SystemRoles.Patient);
            users["patient2"] = await CreateUserAsync("jean.dupont@medoffice.local", "Jean", "Dupont", SystemRoles.Patient);
            users["patient3"] = await CreateUserAsync("sarah.lefebvre@medoffice.local", "Sarah", "Lefebvre", SystemRoles.Patient);
            users["patient4"] = await CreateUserAsync("marc.roux@medoffice.local", "Marc", "Roux", SystemRoles.Patient);
            users["patient5"] = await CreateUserAsync("lucie.garcia@medoffice.local", "Lucie", "Garcia", SystemRoles.Patient);

            return users;
        }

        private static async Task<List<Medecin>> SeedMedecinsAsync(
            MedicalOfficeContext context, 
            Dictionary<string, ApplicationUser> users)
        {
            // #region agent log
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), ".cursor", "debug.log");
            try { await File.AppendAllTextAsync(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"DatabaseSeeder.cs:142\",\"message\":\"SeedMedecinsAsync entry\",\"data\":{{\"hasExistingMedecins\":{await context.Medecins.AnyAsync()}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            if (await context.Medecins.AnyAsync())
            {
                var existingMedecins = await context.Medecins.ToListAsync();
                // #region agent log
                try { await File.AppendAllTextAsync(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"DatabaseSeeder.cs:144\",\"message\":\"Returning existing medecins\",\"data\":{{\"count\":{existingMedecins.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                return existingMedecins;
            }

            var medecins = new List<Medecin>
            {
                new Medecin
                {
                    ApplicationUserId = users["medecin1"].Id,
                    NomPrenom = "Dr. Claire Leclerc",
                    Specialite = "Médecine générale",
                    Adresse = "12 Rue de la Santé, 75014 Paris",
                    Telephone = "+33155501010",
                    Email = "claire.leclerc@medoffice.local"
                },
                new Medecin
                {
                    ApplicationUserId = users["medecin2"].Id,
                    NomPrenom = "Dr. Pierre Dubois",
                    Specialite = "Cardiologie",
                    Adresse = "45 Avenue des Champs, 75008 Paris",
                    Telephone = "+33155501011",
                    Email = "pierre.dubois@medoffice.local"
                },
                new Medecin
                {
                    ApplicationUserId = users["medecin3"].Id,
                    NomPrenom = "Dr. Anne Bernard",
                    Specialite = "Pédiatrie",
                    Adresse = "78 Boulevard Saint-Germain, 75006 Paris",
                    Telephone = "+33155501012",
                    Email = "anne.bernard@medoffice.local"
                },
                new Medecin
                {
                    ApplicationUserId = users["medecin4"].Id,
                    NomPrenom = "Dr. Thomas Moreau",
                    Specialite = "Dermatologie",
                    Adresse = "23 Rue de Rivoli, 75001 Paris",
                    Telephone = "+33155501013",
                    Email = "thomas.moreau@medoffice.local"
                }
            };

            context.Medecins.AddRange(medecins);
            await context.SaveChangesAsync();
            // #region agent log
            var logPath2 = Path.Combine(Directory.GetCurrentDirectory(), ".cursor", "debug.log");
            try { await File.AppendAllTextAsync(logPath2, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"A\",\"location\":\"DatabaseSeeder.cs:197\",\"message\":\"Returning new medecins\",\"data\":{{\"count\":{medecins.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            return medecins;
        }

        private static async Task<List<Patient>> SeedPatientsAsync(
            MedicalOfficeContext context, 
            Dictionary<string, ApplicationUser> users)
        {
            // #region agent log
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), ".cursor", "debug.log");
            try { await File.AppendAllTextAsync(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"DatabaseSeeder.cs:196\",\"message\":\"SeedPatientsAsync entry\",\"data\":{{\"hasExistingPatients\":{await context.Patients.AnyAsync()}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            if (await context.Patients.AnyAsync())
            {
                // Use IgnoreQueryFilters to get all patients including deleted ones for seeding purposes
                var existingPatients = await context.Patients.IgnoreQueryFilters().ToListAsync();
                // #region agent log
                try { await File.AppendAllTextAsync(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"DatabaseSeeder.cs:198\",\"message\":\"Returning existing patients\",\"data\":{{\"count\":{existingPatients.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
                // #endregion
                return existingPatients;
            }

            var now = DateTime.UtcNow;
            var patients = new List<Patient>
            {
                new Patient
                {
                    ApplicationUserId = users["patient1"].Id,
                    Nom = "Martin",
                    Prenom = "Alice",
                    DateNaissance = new DateOnly(1985, 3, 15),
                    Sexe = "F",
                    Adresse = "5 Avenue des Lilas, 75012 Paris",
                    Telephone = "+33155500001",
                    Email = "alice.martin@medoffice.local",
                    Antecedents = "Hypertension légère, allergies aux pénicillines",
                    CreatedAt = now.AddMonths(-6),
                    UpdatedAt = now.AddDays(-5),
                    CreatedBy = "admin@medoffice.local",
                    UpdatedBy = "marie.dupont@medoffice.local",
                    IsDeleted = false
                },
                new Patient
                {
                    ApplicationUserId = users["patient2"].Id,
                    Nom = "Dupont",
                    Prenom = "Jean",
                    DateNaissance = new DateOnly(1978, 7, 22),
                    Sexe = "M",
                    Adresse = "12 Rue de la République, 75011 Paris",
                    Telephone = "+33155500002",
                    Email = "jean.dupont@medoffice.local",
                    Antecedents = "Diabète type 2, suivi régulier",
                    CreatedAt = now.AddMonths(-4),
                    UpdatedAt = now.AddDays(-2),
                    CreatedBy = "marie.dupont@medoffice.local",
                    UpdatedBy = "marie.dupont@medoffice.local",
                    IsDeleted = false
                },
                new Patient
                {
                    ApplicationUserId = users["patient3"].Id,
                    Nom = "Lefebvre",
                    Prenom = "Sarah",
                    DateNaissance = new DateOnly(1992, 11, 8),
                    Sexe = "F",
                    Adresse = "28 Boulevard Voltaire, 75011 Paris",
                    Telephone = "+33155500003",
                    Email = "sarah.lefebvre@medoffice.local",
                    Antecedents = "Aucun antécédent notable",
                    CreatedAt = now.AddMonths(-3),
                    UpdatedAt = now.AddDays(-10),
                    CreatedBy = "sophie.martin@medoffice.local",
                    UpdatedBy = "sophie.martin@medoffice.local",
                    IsDeleted = false
                },
                new Patient
                {
                    ApplicationUserId = users["patient4"].Id,
                    Nom = "Roux",
                    Prenom = "Marc",
                    DateNaissance = new DateOnly(1965, 5, 30),
                    Sexe = "M",
                    Adresse = "15 Rue de Vaugirard, 75006 Paris",
                    Telephone = "+33155500004",
                    Email = "marc.roux@medoffice.local",
                    Antecedents = "Antécédents cardiaques familiaux, cholestérol élevé",
                    CreatedAt = now.AddMonths(-8),
                    UpdatedAt = now.AddDays(-1),
                    CreatedBy = "admin@medoffice.local",
                    UpdatedBy = "marie.dupont@medoffice.local",
                    IsDeleted = false
                },
                new Patient
                {
                    ApplicationUserId = users["patient5"].Id,
                    Nom = "Garcia",
                    Prenom = "Lucie",
                    DateNaissance = new DateOnly(2005, 9, 14),
                    Sexe = "F",
                    Adresse = "42 Avenue de la Grande Armée, 75016 Paris",
                    Telephone = "+33155500005",
                    Email = "lucie.garcia@medoffice.local",
                    Antecedents = "Asthme infantile, suivi pédiatrique",
                    CreatedAt = now.AddMonths(-2),
                    UpdatedAt = now.AddDays(-3),
                    CreatedBy = "sophie.martin@medoffice.local",
                    UpdatedBy = "sophie.martin@medoffice.local",
                    IsDeleted = false
                },
                // Additional patients without user accounts
                new Patient
                {
                    Nom = "Bernard",
                    Prenom = "Sophie",
                    DateNaissance = new DateOnly(1990, 2, 18),
                    Sexe = "F",
                    Adresse = "8 Rue de la Paix, 75002 Paris",
                    Telephone = "+33155500006",
                    Email = "sophie.bernard@example.com",
                    Antecedents = "Migraines récurrentes",
                    CreatedAt = now.AddMonths(-1),
                    UpdatedAt = now.AddDays(-7),
                    CreatedBy = "marie.dupont@medoffice.local",
                    UpdatedBy = "marie.dupont@medoffice.local",
                    IsDeleted = false
                },
                new Patient
                {
                    Nom = "Petit",
                    Prenom = "Michel",
                    DateNaissance = new DateOnly(1972, 12, 5),
                    Sexe = "M",
                    Adresse = "33 Rue de Belleville, 75020 Paris",
                    Telephone = "+33155500007",
                    Email = "michel.petit@example.com",
                    Antecedents = "Hypertension artérielle",
                    CreatedAt = now.AddMonths(-5),
                    UpdatedAt = now.AddDays(-15),
                    CreatedBy = "admin@medoffice.local",
                    UpdatedBy = "sophie.martin@medoffice.local",
                    IsDeleted = false
                },
                new Patient
                {
                    Nom = "Leroy",
                    Prenom = "Emma",
                    DateNaissance = new DateOnly(1988, 6, 25),
                    Sexe = "F",
                    Adresse = "19 Rue de la Sorbonne, 75005 Paris",
                    Telephone = "+33155500008",
                    Email = "emma.leroy@example.com",
                    Antecedents = "Aucun",
                    CreatedAt = now.AddMonths(-7),
                    UpdatedAt = now.AddDays(-20),
                    CreatedBy = "marie.dupont@medoffice.local",
                    UpdatedBy = "marie.dupont@medoffice.local",
                    IsDeleted = false
                }
            };

            context.Patients.AddRange(patients);
            await context.SaveChangesAsync();
            // #region agent log
            var logPath3 = Path.Combine(Directory.GetCurrentDirectory(), ".cursor", "debug.log");
            try { await File.AppendAllTextAsync(logPath3, $"{{\"sessionId\":\"debug-session\",\"runId\":\"run1\",\"hypothesisId\":\"B\",\"location\":\"DatabaseSeeder.cs:361\",\"message\":\"Returning new patients\",\"data\":{{\"count\":{patients.Count}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            return patients;
        }

        private static async Task<List<RendezVou>> SeedAppointmentsAsync(
            MedicalOfficeContext context,
            List<Patient> patients,
            List<Medecin> medecins)
        {
            // #region agent log
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), ".cursor", "debug.log");
            try { await File.AppendAllTextAsync(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"post-fix\",\"hypothesisId\":\"A,B,C\",\"location\":\"DatabaseSeeder.cs:374\",\"message\":\"SeedAppointmentsAsync entry\",\"data\":{{\"patientsCount\":{patients?.Count ?? -1},\"medecinsCount\":{medecins?.Count ?? -1},\"patientsIsNull\":{patients == null},\"medecinsIsNull\":{medecins == null}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            if (await context.RendezVous.AnyAsync())
            {
                return await context.RendezVous.ToListAsync();
            }

            // Validate we have enough patients and medecins
            if (patients == null || patients.Count == 0)
            {
                throw new InvalidOperationException("Cannot seed appointments: no patients available.");
            }
            if (medecins == null || medecins.Count == 0)
            {
                throw new InvalidOperationException("Cannot seed appointments: no medecins available.");
            }

            var now = DateTime.Now;
            var appointments = new List<RendezVou>();

            // Helper function to safely get patient index (wraps around if needed)
            int GetPatientIndex(int desiredIndex) => desiredIndex % patients.Count;
            // Helper function to safely get medecin index (wraps around if needed)
            int GetMedecinIndex(int desiredIndex) => desiredIndex % medecins.Count;

            // #region agent log
            try { await File.AppendAllTextAsync(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"post-fix\",\"hypothesisId\":\"B\",\"location\":\"DatabaseSeeder.cs:402\",\"message\":\"Before accessing patients[0] (safe)\",\"data\":{{\"patientsCount\":{patients.Count},\"actualIndex\":{GetPatientIndex(0)}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            // Today's appointments
            // #region agent log
            try { await File.AppendAllTextAsync(logPath, $"{{\"sessionId\":\"debug-session\",\"runId\":\"post-fix\",\"hypothesisId\":\"A\",\"location\":\"DatabaseSeeder.cs:407\",\"message\":\"Before accessing medecins[0] (safe)\",\"data\":{{\"medecinsCount\":{medecins.Count},\"actualIndex\":{GetMedecinIndex(0)}}},\"timestamp\":{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}}}\n"); } catch { }
            // #endregion
            appointments.Add(new RendezVou
            {
                PatientId = patients[GetPatientIndex(0)].Id,
                MedecinId = medecins[GetMedecinIndex(0)].Id,
                SalleId = 1,
                DateDebut = now.Date.AddHours(9),
                DateFin = now.Date.AddHours(9).AddMinutes(30),
                Statut = "Scheduled",
                Motif = "Consultation de routine"
            });

            appointments.Add(new RendezVou
            {
                PatientId = patients[GetPatientIndex(1)].Id,
                MedecinId = medecins[GetMedecinIndex(1)].Id,
                SalleId = 2,
                DateDebut = now.Date.AddHours(10),
                DateFin = now.Date.AddHours(10).AddMinutes(45),
                Statut = "Checked-in",
                Motif = "Suivi cardiaque"
            });

            appointments.Add(new RendezVou
            {
                PatientId = patients[GetPatientIndex(2)].Id,
                MedecinId = medecins[GetMedecinIndex(2)].Id,
                SalleId = 3,
                DateDebut = now.Date.AddHours(11),
                DateFin = now.Date.AddHours(11).AddMinutes(30),
                Statut = "In Progress",
                Motif = "Examen pédiatrique"
            });

            // Tomorrow's appointments
            appointments.Add(new RendezVou
            {
                PatientId = patients[GetPatientIndex(3)].Id,
                MedecinId = medecins[GetMedecinIndex(1)].Id,
                SalleId = 2,
                DateDebut = now.Date.AddDays(1).AddHours(9),
                DateFin = now.Date.AddDays(1).AddHours(9).AddMinutes(30),
                Statut = "Confirmed",
                Motif = "Consultation cardiologique"
            });

            appointments.Add(new RendezVou
            {
                PatientId = patients[GetPatientIndex(4)].Id,
                MedecinId = medecins[GetMedecinIndex(2)].Id,
                SalleId = 3,
                DateDebut = now.Date.AddDays(1).AddHours(14),
                DateFin = now.Date.AddDays(1).AddHours(14).AddMinutes(30),
                Statut = "Scheduled",
                Motif = "Vaccination"
            });

            // Pending approval appointments
            appointments.Add(new RendezVou
            {
                PatientId = patients[GetPatientIndex(5)].Id,
                MedecinId = medecins[GetMedecinIndex(0)].Id,
                SalleId = 1,
                DateDebut = now.Date.AddDays(2).AddHours(10),
                DateFin = now.Date.AddDays(2).AddHours(10).AddMinutes(30),
                Statut = "Pending Approval",
                Motif = "Première consultation"
            });

            appointments.Add(new RendezVou
            {
                PatientId = patients[GetPatientIndex(6)].Id,
                MedecinId = medecins[GetMedecinIndex(3)].Id,
                SalleId = 4,
                DateDebut = now.Date.AddDays(1).AddHours(15),
                DateFin = now.Date.AddDays(1).AddHours(15).AddMinutes(45),
                Statut = "Pending Approval",
                Motif = "Examen dermatologique"
            });

            appointments.Add(new RendezVou
            {
                PatientId = patients[GetPatientIndex(7)].Id,
                MedecinId = medecins[GetMedecinIndex(0)].Id,
                SalleId = 1,
                DateDebut = now.Date.AddHours(16),
                DateFin = now.Date.AddHours(16).AddMinutes(30),
                Statut = "Pending Approval",
                Motif = "Consultation urgente"
            });

            // Upcoming appointments
            for (int i = 0; i < 10; i++)
            {
                var patient = patients[new Random().Next(patients.Count)];
                var medecin = medecins[new Random().Next(medecins.Count)];
                var daysAhead = new Random().Next(3, 14);
                var hour = new Random().Next(9, 17);
                var statuses = new[] { "Scheduled", "Confirmed" };
                var status = statuses[new Random().Next(statuses.Length)];

                appointments.Add(new RendezVou
                {
                    PatientId = patient.Id,
                    MedecinId = medecin.Id,
                    SalleId = new Random().Next(1, 5),
                    DateDebut = now.Date.AddDays(daysAhead).AddHours(hour),
                    DateFin = now.Date.AddDays(daysAhead).AddHours(hour).AddMinutes(30),
                    Statut = status,
                    Motif = new[] { "Consultation de routine", "Suivi médical", "Examen", "Vaccination", "Contrôle" }[new Random().Next(5)]
                });
            }

            // Past completed appointments
            for (int i = 0; i < 8; i++)
            {
                var patient = patients[new Random().Next(patients.Count)];
                var medecin = medecins[new Random().Next(medecins.Count)];
                var daysAgo = new Random().Next(1, 30);
                var hour = new Random().Next(9, 17);

                appointments.Add(new RendezVou
                {
                    PatientId = patient.Id,
                    MedecinId = medecin.Id,
                    SalleId = new Random().Next(1, 5),
                    DateDebut = now.Date.AddDays(-daysAgo).AddHours(hour),
                    DateFin = now.Date.AddDays(-daysAgo).AddHours(hour).AddMinutes(30),
                    Statut = "Completed",
                    Motif = new[] { "Consultation", "Examen", "Suivi", "Contrôle" }[new Random().Next(4)]
                });
            }

            context.RendezVous.AddRange(appointments);
            await context.SaveChangesAsync();
            return appointments;
        }

        private static async Task SeedConsultationsAsync(
            MedicalOfficeContext context,
            List<Patient> patients,
            List<Medecin> medecins,
            List<RendezVou> appointments)
        {
            if (await context.Consultations.AnyAsync())
            {
                return;
            }

            var completedAppointments = appointments.Where(a => a.Statut == "Completed").ToList();
            var consultations = new List<Consultation>();

            foreach (var appointment in completedAppointments.Take(5))
            {
                consultations.Add(new Consultation
                {
                    PatientId = appointment.PatientId,
                    MedecinId = appointment.MedecinId,
                    RendezVousId = appointment.Id,
                    DateConsult = appointment.DateDebut,
                    Observations = "Patient en bonne santé générale. Tension artérielle normale. Pas de signes d'alerte.",
                    Diagnostics = "Examen normal, aucun problème détecté"
                });
            }

            // Additional consultations not linked to appointments
            for (int i = 0; i < 3; i++)
            {
                var patient = patients[new Random().Next(patients.Count)];
                var medecin = medecins[new Random().Next(medecins.Count)];
                var daysAgo = new Random().Next(5, 20);

                consultations.Add(new Consultation
                {
                    PatientId = patient.Id,
                    MedecinId = medecin.Id,
                    RendezVousId = null,
                    DateConsult = DateTime.Now.AddDays(-daysAgo),
                    Observations = "Consultation de suivi. Patient suit bien le traitement prescrit.",
                    Diagnostics = "Amélioration des symptômes, poursuite du traitement"
                });
            }

            context.Consultations.AddRange(consultations);
            await context.SaveChangesAsync();
        }

        private static async Task SeedPrescriptionsAsync(
            MedicalOfficeContext context,
            List<Patient> patients,
            List<Medecin> medecins)
        {
            if (await context.Prescriptions.AnyAsync())
            {
                return;
            }

            var prescriptions = new List<Prescription>
            {
                new Prescription
                {
                    PatientId = patients[0].Id,
                    MedecinId = medecins[0].Id,
                    Medication = "Amoxicilline",
                    Dosage = "500mg",
                    Frequency = "3 fois par jour",
                    Status = "Active",
                    IssuedOn = DateTime.UtcNow.AddDays(-5),
                    NextRefill = DateTime.UtcNow.AddDays(5),
                    RefillsRemaining = 1,
                    Notes = "Prendre avec les repas. Terminer le traitement complet."
                },
                new Prescription
                {
                    PatientId = patients[1].Id,
                    MedecinId = medecins[1].Id,
                    Medication = "Atorvastatine",
                    Dosage = "20mg",
                    Frequency = "1 fois par jour, le soir",
                    Status = "Active",
                    IssuedOn = DateTime.UtcNow.AddDays(-30),
                    NextRefill = DateTime.UtcNow.AddDays(30),
                    RefillsRemaining = 2,
                    Notes = "Traitement chronique pour le cholestérol"
                },
                new Prescription
                {
                    PatientId = patients[2].Id,
                    MedecinId = medecins[2].Id,
                    Medication = "Paracétamol",
                    Dosage = "500mg",
                    Frequency = "Si nécessaire, maximum 3g/jour",
                    Status = "Active",
                    IssuedOn = DateTime.UtcNow.AddDays(-10),
                    NextRefill = null,
                    RefillsRemaining = 0,
                    Notes = "Pour douleurs légères"
                },
                new Prescription
                {
                    PatientId = patients[4].Id,
                    MedecinId = medecins[2].Id,
                    Medication = "Ventoline",
                    Dosage = "100mcg",
                    Frequency = "2 bouffées en cas de crise",
                    Status = "Active",
                    IssuedOn = DateTime.UtcNow.AddDays(-15),
                    NextRefill = DateTime.UtcNow.AddDays(45),
                    RefillsRemaining = 1,
                    Notes = "Traitement de l'asthme. À utiliser en cas de besoin."
                },
                new Prescription
                {
                    PatientId = patients[3].Id,
                    MedecinId = medecins[1].Id,
                    Medication = "Aspirine",
                    Dosage = "100mg",
                    Frequency = "1 fois par jour",
                    Status = "Active",
                    IssuedOn = DateTime.UtcNow.AddDays(-20),
                    NextRefill = DateTime.UtcNow.AddDays(10),
                    RefillsRemaining = 0,
                    Notes = "Prévention cardiovasculaire"
                }
            };

            context.Prescriptions.AddRange(prescriptions);
            await context.SaveChangesAsync();
        }

        private static async Task SeedLabResultsAsync(
            MedicalOfficeContext context,
            List<Patient> patients,
            List<Medecin> medecins)
        {
            if (await context.LabResults.AnyAsync())
            {
                return;
            }

            var labResults = new List<LabResult>
            {
                new LabResult
                {
                    PatientId = patients[0].Id,
                    MedecinId = medecins[0].Id,
                    TestName = "Numération formule sanguine (NFS)",
                    Priority = "Routine",
                    Status = "Completed",
                    CollectedOn = DateTime.UtcNow.AddDays(-3),
                    ResultValue = "Dans les limites normales",
                    Notes = "Tous les paramètres sont normaux"
                },
                new LabResult
                {
                    PatientId = patients[1].Id,
                    MedecinId = medecins[1].Id,
                    TestName = "Cholestérol total et fractions",
                    Priority = "Routine",
                    Status = "Completed",
                    CollectedOn = DateTime.UtcNow.AddDays(-7),
                    ResultValue = "Cholestérol total: 5.2 mmol/L, LDL: 3.1 mmol/L",
                    Notes = "Légèrement élevé, traitement en cours"
                },
                new LabResult
                {
                    PatientId = patients[1].Id,
                    MedecinId = medecins[1].Id,
                    TestName = "Glycémie à jeun",
                    Priority = "Routine",
                    Status = "Completed",
                    CollectedOn = DateTime.UtcNow.AddDays(-7),
                    ResultValue = "5.8 mmol/L",
                    Notes = "Bien contrôlé sous traitement"
                },
                new LabResult
                {
                    PatientId = patients[2].Id,
                    MedecinId = medecins[2].Id,
                    TestName = "Test de grossesse",
                    Priority = "Routine",
                    Status = "Completed",
                    CollectedOn = DateTime.UtcNow.AddDays(-1),
                    ResultValue = "Négatif",
                    Notes = "Résultat négatif"
                },
                new LabResult
                {
                    PatientId = patients[3].Id,
                    MedecinId = medecins[1].Id,
                    TestName = "Troponine I",
                    Priority = "STAT",
                    Status = "Pending Review",
                    CollectedOn = DateTime.UtcNow.AddHours(-2),
                    ResultValue = "0.03 ng/mL",
                    Notes = "Résultat normal, à revoir si symptômes persistent"
                },
                new LabResult
                {
                    PatientId = patients[4].Id,
                    MedecinId = medecins[2].Id,
                    TestName = "Test d'allergie",
                    Priority = "Routine",
                    Status = "In Progress",
                    CollectedOn = DateTime.UtcNow.AddDays(-2),
                    ResultValue = null,
                    Notes = "En attente des résultats"
                }
            };

            context.LabResults.AddRange(labResults);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBillingInvoicesAsync(
            MedicalOfficeContext context,
            List<Patient> patients)
        {
            if (await context.BillingInvoices.AnyAsync())
            {
                return;
            }

            var invoices = new List<BillingInvoice>
            {
                new BillingInvoice
                {
                    InvoiceNumber = "INV-2024-001",
                    PatientName = $"{patients[0].Prenom} {patients[0].Nom}",
                    Service = "Consultation de routine",
                    Amount = 25.00m,
                    Status = "Paid",
                    IssuedOn = DateTime.Today.AddDays(-10),
                    DueDate = DateTime.Today.AddDays(-2),
                    PaymentMethod = "Carte bancaire"
                },
                new BillingInvoice
                {
                    InvoiceNumber = "INV-2024-002",
                    PatientName = $"{patients[1].Prenom} {patients[1].Nom}",
                    Service = "Consultation cardiologique + ECG",
                    Amount = 85.00m,
                    Status = "Paid",
                    IssuedOn = DateTime.Today.AddDays(-15),
                    DueDate = DateTime.Today.AddDays(-1),
                    PaymentMethod = "Assurance"
                },
                new BillingInvoice
                {
                    InvoiceNumber = "INV-2024-003",
                    PatientName = $"{patients[2].Prenom} {patients[2].Nom}",
                    Service = "Examen pédiatrique",
                    Amount = 30.00m,
                    Status = "Pending",
                    IssuedOn = DateTime.Today.AddDays(-5),
                    DueDate = DateTime.Today.AddDays(9),
                    PaymentMethod = "Assurance"
                },
                new BillingInvoice
                {
                    InvoiceNumber = "INV-2024-004",
                    PatientName = $"{patients[3].Prenom} {patients[3].Nom}",
                    Service = "Consultation + Analyses de laboratoire",
                    Amount = 120.00m,
                    Status = "Pending",
                    IssuedOn = DateTime.Today.AddDays(-3),
                    DueDate = DateTime.Today.AddDays(11),
                    PaymentMethod = "Virement bancaire"
                },
                new BillingInvoice
                {
                    InvoiceNumber = "INV-2024-005",
                    PatientName = $"{patients[4].Prenom} {patients[4].Nom}",
                    Service = "Vaccination",
                    Amount = 15.00m,
                    Status = "Paid",
                    IssuedOn = DateTime.Today.AddDays(-8),
                    DueDate = DateTime.Today,
                    PaymentMethod = "Espèces"
                },
                new BillingInvoice
                {
                    InvoiceNumber = "INV-2024-006",
                    PatientName = $"{patients[5].Prenom} {patients[5].Nom}",
                    Service = "Consultation générale",
                    Amount = 25.00m,
                    Status = "Overdue",
                    IssuedOn = DateTime.Today.AddDays(-25),
                    DueDate = DateTime.Today.AddDays(-11),
                    PaymentMethod = "Carte bancaire"
                },
                new BillingInvoice
                {
                    InvoiceNumber = "INV-2024-007",
                    PatientName = $"{patients[6].Prenom} {patients[6].Nom}",
                    Service = "Consultation dermatologique",
                    Amount = 50.00m,
                    Status = "Pending",
                    IssuedOn = DateTime.Today.AddDays(-2),
                    DueDate = DateTime.Today.AddDays(12),
                    PaymentMethod = "Assurance"
                }
            };

            context.BillingInvoices.AddRange(invoices);
            await context.SaveChangesAsync();
        }

        private static async Task SeedInventoryItemsAsync(MedicalOfficeContext context)
        {
            if (await context.InventoryItems.AnyAsync())
            {
                return;
            }

            var items = new List<InventoryItem>
            {
                new InventoryItem
                {
                    ItemName = "Masques chirurgicaux",
                    Category = "PPE",
                    Quantity = 1250,
                    ReorderLevel = 500,
                    Status = "In Stock"
                },
                new InventoryItem
                {
                    ItemName = "Gants nitrile",
                    Category = "PPE",
                    Quantity = 320,
                    ReorderLevel = 250,
                    Status = "In Stock"
                },
                new InventoryItem
                {
                    ItemName = "Cathéters IV",
                    Category = "Matériel médical",
                    Quantity = 45,
                    ReorderLevel = 100,
                    Status = "Low Stock"
                },
                new InventoryItem
                {
                    ItemName = "Seringues 5ml",
                    Category = "Matériel médical",
                    Quantity = 180,
                    ReorderLevel = 200,
                    Status = "Low Stock"
                },
                new InventoryItem
                {
                    ItemName = "Aiguilles 21G",
                    Category = "Matériel médical",
                    Quantity = 95,
                    ReorderLevel = 150,
                    Status = "Reorder"
                },
                new InventoryItem
                {
                    ItemName = "Bandages stériles",
                    Category = "Pansements",
                    Quantity = 420,
                    ReorderLevel = 300,
                    Status = "In Stock"
                },
                new InventoryItem
                {
                    ItemName = "Antiseptique (Chlorhexidine)",
                    Category = "Médicaments",
                    Quantity = 28,
                    ReorderLevel = 50,
                    Status = "Reorder"
                },
                new InventoryItem
                {
                    ItemName = "Thermomètres digitaux",
                    Category = "Équipement",
                    Quantity = 12,
                    ReorderLevel = 10,
                    Status = "In Stock"
                },
                new InventoryItem
                {
                    ItemName = "Tensiomètres",
                    Category = "Équipement",
                    Quantity = 8,
                    ReorderLevel = 5,
                    Status = "In Stock"
                },
                new InventoryItem
                {
                    ItemName = "Stéthoscopes",
                    Category = "Équipement",
                    Quantity = 15,
                    ReorderLevel = 10,
                    Status = "In Stock"
                }
            };

            context.InventoryItems.AddRange(items);
            await context.SaveChangesAsync();
        }

        private static async Task SeedReportArtifactsAsync(MedicalOfficeContext context)
        {
            if (await context.ReportArtifacts.AnyAsync())
            {
                return;
            }

            var reports = new List<ReportArtifact>
            {
                new ReportArtifact
                {
                    Title = "Rapport de revenus mensuel",
                    Period = "Derniers 30 jours",
                    Owner = "Finance",
                    Status = "Ready",
                    GeneratedOn = DateTime.Today.AddDays(-1)
                },
                new ReportArtifact
                {
                    Title = "Flux de patients",
                    Period = "Cette semaine",
                    Owner = "Opérations",
                    Status = "Ready",
                    GeneratedOn = DateTime.Today
                },
                new ReportArtifact
                {
                    Title = "Utilisation des médicaments",
                    Period = "Trimestre en cours",
                    Owner = "Pharmacie",
                    Status = "In Progress",
                    GeneratedOn = DateTime.Today.AddDays(-3)
                },
                new ReportArtifact
                {
                    Title = "Rapport d'activité médicale",
                    Period = "Mois dernier",
                    Owner = "Médecins",
                    Status = "Ready",
                    GeneratedOn = DateTime.Today.AddDays(-5)
                },
                new ReportArtifact
                {
                    Title = "Analyse des rendez-vous",
                    Period = "Dernière semaine",
                    Owner = "Secrétariat",
                    Status = "Scheduled",
                    GeneratedOn = DateTime.Today.AddDays(-2)
                },
                new ReportArtifact
                {
                    Title = "Rapport de facturation",
                    Period = "Ce mois",
                    Owner = "Finance",
                    Status = "Ready",
                    GeneratedOn = DateTime.Today
                }
            };

            context.ReportArtifacts.AddRange(reports);
            await context.SaveChangesAsync();
        }
    }
}
