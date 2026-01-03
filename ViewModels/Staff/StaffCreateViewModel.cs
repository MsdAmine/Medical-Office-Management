using System.ComponentModel.DataAnnotations;
using MedicalOfficeManagement.Models.Security;

namespace MedicalOfficeManagement.ViewModels.Staff;

public class StaffCreateViewModel
{
    [Required]
    public string Role { get; set; } = SystemRoles.Secretaire;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 4)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    [Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Mailing address")]
    public string? Address { get; set; }

    [Display(Name = "Specialty (for doctors)")]
    public string? Specialite { get; set; }

    [Display(Name = "Office address (for doctors)")]
    public string? MedecinAdresse { get; set; }

    [Phone]
    [Display(Name = "Office phone (for doctors)")]
    public string? MedecinTelephone { get; set; }
}
