using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace MedicalOfficeManagement.Models
{
    public class Medecin
    {
        public int Id { get; set; }

        [Required]
        public string NomPrenom { get; set; } = null!; // Utilisé pour le "Full Name" du formulaire

        [Required]
        public string Specialite { get; set; } = null!;

        [Required]
        public string Adresse { get; set; } = null!;

        [Required]
        [Phone]
        public string Telephone { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [ValidateNever]
        public string ApplicationUserId { get; set; } = null!;

        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
    }
}