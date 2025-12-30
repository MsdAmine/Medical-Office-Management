using Microsoft.AspNetCore.Identity;

namespace MedicalOfficeManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Nouveaux champs pour l'administrateur
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Address { get; set; }
    }
}