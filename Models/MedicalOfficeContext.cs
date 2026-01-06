using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
// using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General; <-- SUPPRIMÉ
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

// Hérite de IdentityDbContext<ApplicationUser>
public partial class MedicalOfficeContext : IdentityDbContext<ApplicationUser>
{
    public MedicalOfficeContext()
    {
    }

    // Le constructeur doit accepter le DbContextOptions générique
    public MedicalOfficeContext(DbContextOptions<MedicalOfficeContext> options)
    : base(options)
    {
    }


    public virtual DbSet<Consultation> Consultations { get; set; } = null!;

    public virtual DbSet<Medecin> Medecins { get; set; } = null!;

    public virtual DbSet<Patient> Patients { get; set; } = null!;

    public virtual DbSet<RendezVou> RendezVous { get; set; } = null!;

    public virtual DbSet<BillingInvoice> BillingInvoices { get; set; } = null!;

    public virtual DbSet<InventoryItem> InventoryItems { get; set; } = null!;

    public virtual DbSet<ReportArtifact> ReportArtifacts { get; set; } = null!;


    // public virtual DbSet<Utilisateur> Utilisateurs { get; set; } // Ligne retirée

    // OnConfiguring est supprimé, la configuration se fera dans Program.cs

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // NOUVEAU : Doit appeler base.OnModelCreating en premier pour configurer l'identité
        base.OnModelCreating(modelBuilder);

        // ... (Toutes les autres configurations de mappage de vos tables)
        modelBuilder.Entity<Patient>()
            .HasQueryFilter(p => !p.IsDeleted);

        ConfigurePatient(modelBuilder.Entity<Patient>());

        // IMPORTANT : Vous devez avoir retiré le mappage de la table Utilisateur.

        OnModelCreatingPartial(modelBuilder);
    }

    private static void ConfigurePatient(EntityTypeBuilder<Patient> patient)
    {
        patient
            .HasOne(p => p.ApplicationUser)
            .WithMany()
            .HasForeignKey(p => p.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);

        patient
            .HasIndex(p => p.ApplicationUserId)
            .IsUnique()
            .HasFilter("[ApplicationUserId] IS NOT NULL");
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
