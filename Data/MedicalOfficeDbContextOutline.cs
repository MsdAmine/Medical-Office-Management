// File: Data/MedicalOfficeDbContextOutline.cs
using MedicalOfficeManagement.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Data
{
    /// <summary>
    /// Future use: DbContext outline for persistent storage, aligned with ViewModel-driven UI.
    /// </summary>
    public class MedicalOfficeDbContext : DbContext
    {
        public MedicalOfficeDbContext(DbContextOptions<MedicalOfficeDbContext> options) : base(options)
        {
        }

        public DbSet<PatientEntity> Patients => Set<PatientEntity>();
        public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();
        public DbSet<DoctorEntity> Doctors => Set<DoctorEntity>();
        public DbSet<InvoiceEntity> Invoices => Set<InvoiceEntity>();
        public DbSet<FilterPresetEntity> FilterPresets => Set<FilterPresetEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PatientEntity>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.MedicalRecordNumber).HasMaxLength(32);
                entity.Property(p => p.FirstName).HasMaxLength(100);
                entity.Property(p => p.LastName).HasMaxLength(100);
                entity.Property(p => p.RiskLevel).HasMaxLength(32);
                entity.Property(p => p.Phone).HasMaxLength(32);
                entity.Property(p => p.Email).HasMaxLength(256);

                entity.HasMany(p => p.Appointments)
                    .WithOne(a => a.Patient)
                    .HasForeignKey(a => a.PatientId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DoctorEntity>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.FullName).IsRequired().HasMaxLength(200);
                entity.Property(d => d.Specialty).IsRequired().HasMaxLength(100);
                entity.Property(d => d.ClinicId).HasMaxLength(64);
                entity.Property(d => d.Email).HasMaxLength(256);
                entity.Property(d => d.Phone).HasMaxLength(32);
                entity.Property(d => d.Location).HasMaxLength(128);

                entity.HasMany(d => d.Appointments)
                    .WithOne(a => a.Doctor)
                    .HasForeignKey(a => a.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AppointmentEntity>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Room).HasMaxLength(64);
                entity.Property(a => a.Status)
                    .HasConversion<string>()
                    .HasMaxLength(32);
                entity.Property(a => a.ClinicId).HasMaxLength(64);
            });

            modelBuilder.Entity<InvoiceEntity>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.Status).HasMaxLength(32);
            });

            modelBuilder.Entity<FilterPresetEntity>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.Scope).HasMaxLength(64);
                entity.Property(f => f.OwnerUserId).HasMaxLength(64);
                entity.Property(f => f.Name).HasMaxLength(128);
            });
        }
    }
}
