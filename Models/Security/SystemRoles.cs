// File: Models/Security/SystemRoles.cs
namespace MedicalOfficeManagement.Models.Security
{
    /// <summary>
    /// Centralized role names for identity and authorization checks.
    /// </summary>
    public static class SystemRoles
    {
        public const string Admin = "Admin";
        public const string Secretaire = "Secretaire";
        public const string Medecin = "Medecin";
        public const string Patient = "Patient";

        public const string AdminOrSecretaire = Admin + "," + Secretaire;
        public const string AdminOrMedecin = Admin + "," + Medecin;
        public const string SchedulingTeam = Admin + "," + Secretaire + "," + Medecin;
        public const string ClinicalTeam = Admin + "," + Secretaire + "," + Medecin;
    }
}
