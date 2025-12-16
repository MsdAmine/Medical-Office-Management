using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalOfficeManagement.Models;

public partial class Medecin
{
    public int Id { get; set; }

    // MODIFICATION : Rendus nullables (ajout du '?') pour passer la validation du modèle
    public string? ApplicationUserId { get; set; }

    public string Nom { get; set; } = null!;
    public string Prenom { get; set; } = null!;
    public string Specialite { get; set; } = null!;

    // Propriétés de navigation des relations
    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public virtual ICollection<Planning> Plannings { get; set; } = new List<Planning>();
    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();

    // MODIFICATION : Rendue nullable (ajout du '?')
    [ForeignKey("ApplicationUserId")]
    public virtual ApplicationUser? ApplicationUser { get; set; }
}