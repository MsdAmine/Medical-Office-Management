using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedicalOfficeManagement.Services.RealTime
{
    [Serializable]
    public abstract class ClinicEventBase
    {
        [Required]
        public string EntityId { get; set; } = string.Empty;

        [Required]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        public string EventType { get; set; } = string.Empty;

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        public IReadOnlyCollection<string> AffectedViews { get; set; } = Array.Empty<string>();
    }

    [Serializable]
    public class AppointmentUpdateDto : ClinicEventBase
    {
        public int AppointmentDisplayId { get; set; }

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Summary { get; set; } = string.Empty;
    }

    [Serializable]
    public class PatientCheckInDto : ClinicEventBase
    {
        public int AppointmentDisplayId { get; set; }

        public string CheckInStatus { get; set; } = "Arrived";

        public string? Room { get; set; }
    }

    [Serializable]
    public class InvoiceStatusUpdateDto : ClinicEventBase
    {
        public string Status { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public DateTimeOffset InvoiceDate { get; set; }
    }

    [Serializable]
    public class DoctorAvailabilityUpdateDto : ClinicEventBase
    {
        [Required]
        public string DoctorId { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        public bool IsAvailable { get; set; }

        public int PatientsToday { get; set; }

        public string ChangeReason { get; set; } = string.Empty;

        public DateTime Date { get; set; }
    }
}
