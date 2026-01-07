using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalOfficeManagement.Models;

public class LabResult
{
    public int Id { get; set; }

    [Required]
    public int PatientId { get; set; }

    public int? MedecinId { get; set; }

    [Required]
    [StringLength(150)]
    public string TestName { get; set; } = string.Empty;

    [StringLength(30)]
    public string Priority { get; set; } = "Routine";

    [StringLength(60)]
    public string Status { get; set; } = "Pending";

    public DateTime CollectedOn { get; set; } = DateTime.UtcNow;

    [StringLength(200)]
    public string? ResultValue { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [ValidateNever]
    public Patient? Patient { get; set; }

    [ValidateNever]
    public Medecin? Medecin { get; set; }

    // Helper properties for enum conversion
    [NotMapped]
    public LabResultStatus StatusEnum
    {
        get => Enum.TryParse<LabResultStatus>(Status, true, out var result) ? result : LabResultStatus.Pending;
        set => Status = value.ToString();
    }

    [NotMapped]
    public LabResultPriority PriorityEnum
    {
        get => Enum.TryParse<LabResultPriority>(Priority, true, out var result) ? result : LabResultPriority.Routine;
        set => Priority = value.ToString();
    }
}
