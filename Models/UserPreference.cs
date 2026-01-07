using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalOfficeManagement.Models
{
    public class UserPreference
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string PreferenceKey { get; set; } = null!;

        [Column(TypeName = "nvarchar(max)")]
        public string? PreferenceValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ValidateNever]
        public ApplicationUser? User { get; set; }
    }
}
