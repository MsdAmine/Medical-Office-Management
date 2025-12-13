using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema; // Ajout requis

namespace MedicalOfficeManagement.Models;

public partial class Medecin
{
    public int Id { get; set; }

    // REMPLACER l'ancien int UtilisateurId par un string
    public string ApplicationUserId { get; set; } = null!; // Doit être null! si requis

    public string Nom { get; set; } = null!;

    // ... autres propriétés (Prenom, Specialite, etc.)

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();
    public virtual ICollection<Planning> Plannings { get; set; } = new List<Planning>();
    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();

    // NOUVEAU : Propriété de navigation pointant vers Identity
    [ForeignKey("ApplicationUserId")]
    public virtual ApplicationUser ApplicationUser { get; set; } = null!;
}