using System;
using System.Collections.Generic;

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

    public virtual Medecin Medecin { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual Salle? Salle { get; set; }
}
