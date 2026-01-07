using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalOfficeManagement.Models;

public class BillingInvoice
{
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public int PatientId { get; set; }

    [Required]
    [StringLength(100)]
    public string PatientName { get; set; } = string.Empty;

    [Required]
    [StringLength(120)]
    public string Service { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Draft";

    [DataType(DataType.Date)]
    public DateTime IssuedOn { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);

    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = string.Empty;

    public int? ConsultationId { get; set; }

    public int? RendezVousId { get; set; }

    [ValidateNever]
    public Patient? Patient { get; set; }

    [ValidateNever]
    public Consultation? Consultation { get; set; }

    [ValidateNever]
    public RendezVou? RendezVous { get; set; }

    // Helper property for enum conversion
    [NotMapped]
    public InvoiceStatus StatusEnum
    {
        get => Enum.TryParse<InvoiceStatus>(Status, true, out var result) ? result : InvoiceStatus.Draft;
        set => Status = value.ToString();
    }
}
