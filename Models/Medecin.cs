using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalOfficeManagement.Models
{
    public class Medecin
    {
        public int Id { get; set; }

        // Correction pour le Nom complet
        [Required(ErrorMessage = "Le nom et prénom est obligatoire.")]
        [StringLength(100)]
        [Display(Name = "Nom et Prénom")]
        public string NomPrenom { get; set; } = null!; // <--- Correction de Nullité

        // Champ spécialité
        [Required(ErrorMessage = "La spécialité est obligatoire.")]
        [Display(Name = "Spécialité")]
        public string Specialite { get; set; } = null!; // <--- Correction de Nullité

        // NOUVEAU : Adresse du cabinet
        [Required(ErrorMessage = "L'adresse est obligatoire.")]
        [StringLength(200)]
        [Display(Name = "Adresse du cabinet")]
        public string Adresse { get; set; } = null!; // <--- Correction de Nullité

        // NOUVEAU : Téléphone
        [Required(ErrorMessage = "Le numéro de téléphone est obligatoire.")]
        [Phone]
        [Display(Name = "Téléphone")]
        public string Telephone { get; set; } = null!; // <--- Correction de Nullité

        // Email
        [Required(ErrorMessage = "L'email est obligatoire.")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!; // <--- Correction de Nullité

        // Clé étrangère vers Identity (pour l'Admin créateur)
        public string ApplicationUserId { get; set; } = null!; // <--- Correction de Nullité

        // Propriété de navigation (nécessite le using Identity dans le DbContext)
        public ApplicationUser ApplicationUser { get; set; } = null!;
    }
}