using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalOfficeManagement.Models;

public class Prescription
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    public int? MedecinId { get; set; }

    [Required]
    [StringLength(180)]
    public string Medication { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Dosage { get; set; }

    [StringLength(120)]
    public string? Frequency { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Pending";

    [StringLength(1000)]
    public string? Notes { get; set; }

    [DataType(DataType.Date)]
    public DateTime IssuedOn { get; set; } = DateTime.UtcNow;

    [DataType(DataType.Date)]
    public DateTime? NextRefill { get; set; }

    [Range(0, 50)]
    public int RefillsRemaining { get; set; }

    [ValidateNever]
    public Patient? Patient { get; set; }

    [ValidateNever]
    public Medecin? Medecin { get; set; }
}
