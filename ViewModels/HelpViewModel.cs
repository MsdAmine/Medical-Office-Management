using System.ComponentModel.DataAnnotations;

namespace MedicalOfficeManagement.ViewModels;

public class HelpViewModel
{
    [Required(ErrorMessage = "Subject is required.")]
    [Display(Name = "Subject")]
    [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
    public string Subject { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [Display(Name = "Message")]
    [StringLength(5000, ErrorMessage = "Message cannot exceed 5000 characters.")]
    public string Message { get; set; } = string.Empty;

    public string? StatusMessage { get; set; }
    public bool IsSuccess { get; set; }
}
