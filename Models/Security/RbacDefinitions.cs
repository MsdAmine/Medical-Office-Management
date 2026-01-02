// File: Models/Security/RbacDefinitions.cs
using System.Collections.Generic;

namespace MedicalOfficeManagement.Models.Security
{
    /// <summary>
    /// Future use: system roles for UI gating and authorization policies.
    /// </summary>
    public enum UserRole
    {
        Admin,
        Secretary,
        Medecin,
        Patient
    }

    /// <summary>
    /// Future use: describes permitted actions for a role across modules.
    /// </summary>
    public class RolePermission
    {
        public UserRole Role { get; set; }
        public bool CanRead { get; set; }
        public bool CanCreate { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }
        public IReadOnlyCollection<string> FeatureFlags { get; set; } = new List<string>(); // Future: granular UI toggles.
    }

    /// <summary>
    /// Future use: static permission matrix for seeding policies.
    /// </summary>
    public static class RolePermissionsMatrix
    {
        public static readonly IReadOnlyCollection<RolePermission> Matrix = new List<RolePermission>
        {
            new RolePermission { Role = UserRole.Admin, CanRead = true, CanCreate = false, CanUpdate = false, CanDelete = false },
            new RolePermission { Role = UserRole.Secretary, CanRead = true, CanCreate = true, CanUpdate = true, CanDelete = false },
            new RolePermission { Role = UserRole.Medecin, CanRead = true, CanCreate = false, CanUpdate = false, CanDelete = false },
            new RolePermission { Role = UserRole.Patient, CanRead = false, CanCreate = false, CanUpdate = false, CanDelete = false }
        };

        /*
         * Razor gating (future pattern, no enforcement yet):
         * @if (User.IsInRole(UserRole.Secretary.ToString()))
         * {
         *     <!-- Render create button -->
         * }
         *
         * ViewModel flags:
         * - Introduce properties like CanManageAppointments, CanManageBilling on page-specific ViewModels.
         * - Populate flags in controllers using RolePermissionsMatrix or policy evaluation.
         *
         * Authorization approach:
         * - Prefer policy-based authorization with claims for clinic/department.
         * - Map UserRole to policies (e.g., "RequireAppointmentWrite" for create/update paths).
         */
    }
}
