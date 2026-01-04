using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalOfficeManagement.Models;

public class ReportArtifact
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Period { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Owner { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "Draft";

    [DataType(DataType.Date)]
    public DateTime GeneratedOn { get; set; } = DateTime.Today;
}
