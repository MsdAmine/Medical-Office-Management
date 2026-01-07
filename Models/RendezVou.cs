using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalOfficeManagement.Models;

public partial class RendezVou
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int MedecinId { get; set; }

    public int? SalleId { get; set; }

    public DateTime DateDebut { get; set; }

    public DateTime DateFin { get; set; }

    public string Statut { get; set; } = null!;

    public string? Motif { get; set; }

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ICollection<BillingInvoice> BillingInvoices { get; set; } = new List<BillingInvoice>();

    public virtual Medecin Medecin { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    // Helper property for enum conversion
    [NotMapped]
    public AppointmentStatus StatusEnum
    {
        get
        {
            // Map legacy string values to enum
            if (string.IsNullOrWhiteSpace(Statut))
                return AppointmentStatus.Scheduled;

            var statusLower = Statut.ToLowerInvariant();
            return statusLower switch
            {
                "scheduled" or "confirmed" => AppointmentStatus.Scheduled,
                "completed" => AppointmentStatus.Completed,
                "cancelled" => AppointmentStatus.Cancelled,
                "pending approval" or "pendingapproval" => AppointmentStatus.PendingApproval,
                "noshow" or "no show" => AppointmentStatus.NoShow,
                _ => Enum.TryParse<AppointmentStatus>(Statut, true, out var result) ? result : AppointmentStatus.Scheduled
            };
        }
        set => Statut = value.ToString();
    }
}
