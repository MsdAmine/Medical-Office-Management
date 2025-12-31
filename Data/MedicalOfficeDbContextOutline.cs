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

        /*
         * Mapping to existing ViewModels (future strategy):
         * - PatientsIndexViewModel <- PatientEntity (projection to list-friendly DTOs with risk flags).
         * - AppointmentsIndexViewModel <- AppointmentEntity (includes PatientEntity + DoctorEntity names).
         * - DoctorsIndexViewModel <- DoctorEntity aggregated with workload buckets (see DoctorWorkloadViewModel).
         * - Billing views <- InvoiceEntity joined with PatientEntity for demographic context.
         * - FilterPresetViewModel <- FilterPresetEntity (CriteriaJson <-> Dictionary<string, string[]> conversion).
         *
         * Data flow diagram (text):
         * - Controllers fetch IQueryable<T> from repositories/services.
         * - Services enforce RBAC and preset application, then project to ViewModels.
         * - ViewModels feed Razor views without exposing EF entities.
         * - Commands (create/update) accept ViewModels/DTOs, map to entities, and persist via DbContext.
         */
    }
}
