using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

public partial class Consultation
{
    public int Id { get; set; }

    public int PatientId { get; set; }

    public int MedecinId { get; set; }

    public int? RendezVousId { get; set; }

    public DateTime DateConsult { get; set; }

    public string? Observations { get; set; }

    public string? Diagnostics { get; set; }

    public virtual Medecin Medecin { get; set; } = null!;

    public virtual Patient Patient { get; set; } = null!;

    public virtual RendezVou? RendezVous { get; set; }
}
