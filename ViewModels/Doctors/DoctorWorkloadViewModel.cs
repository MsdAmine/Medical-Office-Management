// File: ViewModels/Doctors/DoctorWorkloadViewModel.cs
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Doctors
{
    /// <summary>
    /// Future use: supports rendering doctor workload heatmaps on dashboard and doctor list pages.
    /// </summary>
    public class DoctorWorkloadViewModel
    {
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string ClinicId { get; set; } = string.Empty; // Future: partition workloads by clinic/location.
        public List<WorkloadBucketViewModel> TimeBuckets { get; set; } = new();
        public int TotalAppointments { get; set; }
        public int ExpectedCapacity { get; set; } // Future: precomputed capacity for the displayed window.
        public string AdvisoryNote { get; set; } = string.Empty; // Future: data-driven advisory (e.g., "Add coverage").
    }

    /// <summary>
    /// Future use: represents a time-bucketed workload cell for heatmap rendering.
    /// </summary>
    public class WorkloadBucketViewModel
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int ScheduledAppointments { get; set; }
        public int Capacity { get; set; }
        public double LoadFactor { get; set; } // Computed ratio; drives color scale bands.
        public string SeverityBand { get; set; } = string.Empty; // Future: "low", "medium", "high", "critical".
        public string BucketKey { get; set; } = string.Empty; // Future: deterministic key for client-side updates.
    }

    /*
     * Aggregation pseudocode (future implementation):
     * - Input: IQueryable<AppointmentEntity> scoped to doctors/clinic/time range.
     * - Bucket selection: choose interval (e.g., 60 minutes) based on viewport granularity.
     * - Group by doctor + bucket:
     *     var buckets = appointments
     *         .GroupBy(a => new { a.DoctorId, Bucket = TimeTruncate(a.StartTime, bucketMinutes) })
     *         .Select(g => new {
     *             DoctorId = g.Key.DoctorId,
     *             BucketStart = g.Key.Bucket,
     *             Count = g.Count(),
     *             Capacity = g.Max(a => a.CapacityPerSlot) // or schedule-derived capacity
     *         });
     * - LoadFactor = Count / Capacity (default capacity fallback to clinic standard).
     * - SeverityBand selection (Tailwind-friendly tokens):
     *     if (LoadFactor >= 1.0) => "critical"
     *     else if (LoadFactor >= 0.8) => "high"
     *     else if (LoadFactor >= 0.5) => "medium"
     *     else => "low"
     * - Produce DoctorWorkloadViewModel per doctor with ordered buckets for both Dashboard and Doctors/Index.
     */

    /*
     * Rendering strategy notes (no UI yet):
     * - Use Tailwind-compatible classes mapped from SeverityBand (e.g., heatmap-low => bg-emerald-200).
     * - Shared partial for Dashboard and Doctors pages to avoid duplicate heatmap markup.
     * - Accessibility: include text equivalents (LoadFactor, appointment counts) for screen readers.
     */
}
