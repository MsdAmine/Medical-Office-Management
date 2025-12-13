using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

public partial class Medecin
{
    public int Id { get; set; }

    public int UtilisateurId { get; set; }

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public string? Specialite { get; set; }

    public string? Contact { get; set; }

    public string Statut { get; set; } = null!;

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ICollection<Planning> Plannings { get; set; } = new List<Planning>();

    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();

    public virtual Utilisateur Utilisateur { get; set; } = null!;
}
