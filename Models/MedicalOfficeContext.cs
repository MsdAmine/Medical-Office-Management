using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
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

    public virtual DbSet<AuditLog> AuditLogs { get; set; } = null!;

    public virtual DbSet<Consultation> Consultations { get; set; } = null!;

    public virtual DbSet<Medecin> Medecins { get; set; } = null!;

    public virtual DbSet<Ordonnance> Ordonnances { get; set; } = null!;

    public virtual DbSet<Patient> Patients { get; set; } = null!;

    public virtual DbSet<Planning> Plannings { get; set; } = null!;

    public virtual DbSet<RendezVou> RendezVous { get; set; } = null!;

    public virtual DbSet<Salle> Salles { get; set; } = null!;

    // public virtual DbSet<Utilisateur> Utilisateurs { get; set; } // Ligne retirée

    // OnConfiguring est supprimé, la configuration se fera dans Program.cs

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // NOUVEAU : Doit appeler base.OnModelCreating en premier pour configurer l'identité
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__audit_lo__3213E83F5863DEA8");

            entity.ToTable("audit_log");
            // ... le reste du mappage
        });

        // ... (Toutes les autres configurations de mappage de vos tables)

        // IMPORTANT : Vous devez avoir retiré le mappage de la table Utilisateur.

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
