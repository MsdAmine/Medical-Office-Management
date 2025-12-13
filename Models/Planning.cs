using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

public partial class Planning
{
    public int Id { get; set; }

    public int MedecinId { get; set; }

    public DateTime DateDebut { get; set; }

    public DateTime DateFin { get; set; }

    public string StatutDisponibilite { get; set; } = null!;

    public virtual Medecin Medecin { get; set; } = null!;
}
