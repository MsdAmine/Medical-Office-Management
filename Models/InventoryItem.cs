using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalOfficeManagement.Models;

public class InventoryItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(120)]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [StringLength(60)]
    public string Category { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int ReorderLevel { get; set; }

    [Required]
    [StringLength(30)]
    public string Status { get; set; } = "In Stock";
}
