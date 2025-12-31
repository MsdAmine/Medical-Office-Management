// File: Data/Entities/DomainEntities.cs
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Data.Entities
{
    /// <summary>
    /// Future EF Core entity: canonical patient record.
    /// </summary>
    public class PatientEntity
    {
        public Guid Id { get; set; }
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string? RiskLevel { get; set; } // Future: used by saved filters and dashboards.
        public string? PrimaryPhysicianId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public ICollection<AppointmentEntity> Appointments { get; set; } = new List<AppointmentEntity>();
    }

    /// <summary>
    /// Future EF Core entity: appointment linking patients and doctors.
    /// </summary>
    public class AppointmentEntity
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Waiting; // Future: confirm/cancel/waitlist.
        public string? ClinicId { get; set; }
        public string? Room { get; set; }
        public int CapacityPerSlot { get; set; } = 1; // Future: used by workload heatmaps.
        public PatientEntity? Patient { get; set; }
        public DoctorEntity? Doctor { get; set; }
    }

    /// <summary>
    /// Future EF Core entity: clinician roster.
    /// </summary>
    public class DoctorEntity
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string ClinicId { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Location { get; set; }
        public ICollection<AppointmentEntity> Appointments { get; set; } = new List<AppointmentEntity>();
    }

    /// <summary>
    /// Future EF Core entity: invoice/billing header.
    /// </summary>
    public class InvoiceEntity
    {
        public Guid Id { get; set; }
        public Guid PatientId { get; set; }
        public DateTime ServiceDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty; // e.g., Draft/Submitted/Paid.
        public PatientEntity? Patient { get; set; }
    }

    /// <summary>
    /// Future EF Core entity: persisted saved filter presets for personalization.
    /// </summary>
    public class FilterPresetEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty; // Patients, Appointments, etc.
        public string OwnerUserId { get; set; } = string.Empty;
        public string CriteriaJson { get; set; } = "{}"; // Future: JSON representation of FilterPresetViewModel.Criteria.
        public bool IsDefault { get; set; }
        public DateTime LastUsedUtc { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public enum AppointmentStatus
    {
        Waiting = 0,
        Confirmed = 1,
        Completed = 2,
        Cancelled = 3
    }
}
