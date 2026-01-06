using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalOfficeManagement.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Telephone), IsUnique = true)]
public partial class Patient
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nom { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Prenom { get; set; } = null!;

    public DateOnly? DateNaissance { get; set; }

    [StringLength(20)]
    public string? Sexe { get; set; }

    [StringLength(200)]
    public string? Adresse { get; set; }

    [Required]
    [Phone]
    [StringLength(30)]
    public string? Telephone { get; set; }

    [Required]
    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Antecedents { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

    [StringLength(256)]
    public string? CreatedBy { get; set; }

    [StringLength(256)]
    public string? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    [StringLength(256)]
    public string? DeletedBy { get; set; }

    [StringLength(450)]
    public string? ApplicationUserId { get; set; }

    [ValidateNever]
    public ApplicationUser? ApplicationUser { get; set; }

    public virtual ICollection<Consultation> Consultations { get; set; } = new List<Consultation>();

    public virtual ICollection<RendezVou> RendezVous { get; set; } = new List<RendezVou>();
}
