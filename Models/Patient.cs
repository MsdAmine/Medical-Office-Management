using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models;

public partial class Patient
{
    public int Id { get; set; }

    public string Nom { get; set; } = null!;

    public string Prenom { get; set; } = null!;

    public DateOnly? DateNaissance { get; set; }

    public string? Sexe { get; set; }

    public string? Adresse { get; set; }

    public string? Telephone { get; set; }

    public string? Email { get; set; }

    public string? Antecedents { get; set; }

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();
}
