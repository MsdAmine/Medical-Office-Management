using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Models;

public partial class MedicalOfficeContext : DbContext
{
    public MedicalOfficeContext()
    {
    }

    public MedicalOfficeContext(DbContextOptions<MedicalOfficeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<Consultation> Consultations { get; set; }

    public virtual DbSet<Medecin> Medecins { get; set; }

    public virtual DbSet<Ordonnance> Ordonnances { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Planning> Plannings { get; set; }

    public virtual DbSet<RendezVou> RendezVous { get; set; }

    public virtual DbSet<Salle> Salles { get; set; }

    public virtual DbSet<Utilisateur> Utilisateurs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=MSI;Database=Medical_Office;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__audit_lo__3213E83F5863DEA8");

            entity.ToTable("audit_log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Action)
                .HasMaxLength(50)
                .HasColumnName("action");
            entity.Property(e => e.Details).HasColumnName("details");
            entity.Property(e => e.EnregistrementId).HasColumnName("enregistrement_id");
            entity.Property(e => e.Horodatage)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("horodatage");
            entity.Property(e => e.TableCible)
                .HasMaxLength(100)
                .HasColumnName("table_cible");
            entity.Property(e => e.UtilisateurId).HasColumnName("utilisateur_id");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.AuditLogs)
                .HasForeignKey(d => d.UtilisateurId)
                .HasConstraintName("FK_auditlog_user");
        });

        modelBuilder.Entity<Consultation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__consulta__3213E83F85DE9625");

            entity.ToTable("consultations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateConsult)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("date_consult");
            entity.Property(e => e.Diagnostics).HasColumnName("diagnostics");
            entity.Property(e => e.MedecinId).HasColumnName("medecin_id");
            entity.Property(e => e.Observations).HasColumnName("observations");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.RendezVousId).HasColumnName("rendez_vous_id");

            entity.HasOne(d => d.Medecin).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.MedecinId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_consult_medecin");

            entity.HasOne(d => d.Patient).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_consult_patient");

            entity.HasOne(d => d.RendezVous).WithMany(p => p.Consultations)
                .HasForeignKey(d => d.RendezVousId)
                .HasConstraintName("FK_consult_rdv");
        });

        modelBuilder.Entity<Medecin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__medecins__3213E83F10A9B25F");

            entity.ToTable("medecins");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Contact)
                .HasMaxLength(120)
                .HasColumnName("contact");
            entity.Property(e => e.Nom)
                .HasMaxLength(100)
                .HasColumnName("nom");
            entity.Property(e => e.Prenom)
                .HasMaxLength(100)
                .HasColumnName("prenom");
            entity.Property(e => e.Specialite)
                .HasMaxLength(120)
                .HasColumnName("specialite");
            entity.Property(e => e.Statut)
                .HasMaxLength(30)
                .HasDefaultValue("Actif")
                .HasColumnName("statut");
            entity.Property(e => e.UtilisateurId).HasColumnName("utilisateur_id");

            entity.HasOne(d => d.Utilisateur).WithMany(p => p.Medecins)
                .HasForeignKey(d => d.UtilisateurId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_medecins_utilisateur");
        });

        modelBuilder.Entity<Ordonnance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ordonnan__3213E83FAAA8D700");

            entity.ToTable("ordonnances");

            entity.HasIndex(e => e.ConsultationId, "UQ__ordonnan__650FE0FAE2060492").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConsultationId).HasColumnName("consultation_id");
            entity.Property(e => e.Contenu).HasColumnName("contenu");
            entity.Property(e => e.DateCreation)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("date_creation");

            entity.HasOne(d => d.Consultation).WithOne(p => p.Ordonnance)
                .HasForeignKey<Ordonnance>(d => d.ConsultationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ordonnance_consult");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__patients__3213E83F82C1C9BB");

            entity.ToTable("patients");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Adresse)
                .HasMaxLength(300)
                .HasColumnName("adresse");
            entity.Property(e => e.Antecedents).HasColumnName("antecedents");
            entity.Property(e => e.DateNaissance).HasColumnName("date_naissance");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.Nom)
                .HasMaxLength(100)
                .HasColumnName("nom");
            entity.Property(e => e.Prenom)
                .HasMaxLength(100)
                .HasColumnName("prenom");
            entity.Property(e => e.Sexe)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("sexe");
            entity.Property(e => e.Telephone)
                .HasMaxLength(30)
                .HasColumnName("telephone");
        });

        modelBuilder.Entity<Planning>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__planning__3213E83FB8558AEC");

            entity.ToTable("planning");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateDebut)
                .HasPrecision(0)
                .HasColumnName("date_debut");
            entity.Property(e => e.DateFin)
                .HasPrecision(0)
                .HasColumnName("date_fin");
            entity.Property(e => e.MedecinId).HasColumnName("medecin_id");
            entity.Property(e => e.StatutDisponibilite)
                .HasMaxLength(30)
                .HasDefaultValue("Disponible")
                .HasColumnName("statut_disponibilite");

            entity.HasOne(d => d.Medecin).WithMany(p => p.Plannings)
                .HasForeignKey(d => d.MedecinId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_planning_medecin");
        });

        modelBuilder.Entity<RendezVou>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__rendez_v__3213E83F29B60EF0");

            entity.ToTable("rendez_vous");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateDebut)
                .HasPrecision(0)
                .HasColumnName("date_debut");
            entity.Property(e => e.DateFin)
                .HasPrecision(0)
                .HasColumnName("date_fin");
            entity.Property(e => e.MedecinId).HasColumnName("medecin_id");
            entity.Property(e => e.Motif)
                .HasMaxLength(200)
                .HasColumnName("motif");
            entity.Property(e => e.PatientId).HasColumnName("patient_id");
            entity.Property(e => e.SalleId).HasColumnName("salle_id");
            entity.Property(e => e.Statut)
                .HasMaxLength(30)
                .HasDefaultValue("Demande")
                .HasColumnName("statut");

            entity.HasOne(d => d.Medecin).WithMany(p => p.RendezVous)
                .HasForeignKey(d => d.MedecinId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rdv_medecin");

            entity.HasOne(d => d.Patient).WithMany(p => p.RendezVous)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_rdv_patient");

            entity.HasOne(d => d.Salle).WithMany(p => p.RendezVous)
                .HasForeignKey(d => d.SalleId)
                .HasConstraintName("FK_rdv_salle");
        });

        modelBuilder.Entity<Salle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__salles__3213E83F05A6E1BE");

            entity.ToTable("salles");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Capacite).HasColumnName("capacite");
            entity.Property(e => e.Disponibilite)
                .HasMaxLength(30)
                .HasColumnName("disponibilite");
            entity.Property(e => e.Nom)
                .HasMaxLength(100)
                .HasColumnName("nom");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Utilisateur>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__utilisat__3213E83F1486B6B6");

            entity.ToTable("utilisateurs");

            entity.HasIndex(e => e.Email, "UQ_utilisateurs_email").IsUnique();

            entity.HasIndex(e => e.Username, "UQ_utilisateurs_username").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreation)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("date_creation");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.MotDePasse)
                .HasMaxLength(256)
                .HasColumnName("mot_de_passe");
            entity.Property(e => e.Role)
                .HasMaxLength(50)
                .HasColumnName("role");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
