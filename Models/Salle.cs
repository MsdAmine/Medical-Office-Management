using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

public partial class Salle
{
    public int Id { get; set; }

    public string Nom { get; set; } = null!;

    public string? Type { get; set; }

    public int? Capacite { get; set; }

    public string? Disponibilite { get; set; }

    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();
}
