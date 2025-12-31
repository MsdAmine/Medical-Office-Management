// File: ViewModels/Filters/FilterPresetViewModel.cs
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Filters
{
    /// <summary>
    /// Future use: represents a saved filter preset that can be applied to list screens.
    /// </summary>
    public class FilterPresetViewModel
    {
        public string Id { get; set; } = string.Empty; // Future: stable identifier (e.g., GUID/slug).
        public string Name { get; set; } = string.Empty;
        public string OwnerUserId { get; set; } = string.Empty; // Future: personalize presets per user/clinic.
        public string Scope { get; set; } = string.Empty; // Expected values: "Patients", "Appointments", etc.
        public Dictionary<string, string[]> Criteria { get; set; } = new(); // Keyed by field names; values are operator-ready tokens.
        public bool IsDefault { get; set; } // Future: auto-apply on page load.
        public DateTime LastUsedUtc { get; set; }
        public DateTime CreatedUtc { get; set; }
        public string Description { get; set; } = string.Empty; // Future: explain intent to clinicians.
    }

    /// <summary>
    /// Future use: describes the application context when binding a preset to a filterable query.
    /// </summary>
    public class FilterPresetApplicationContext
    {
        public string Scope { get; set; } = string.Empty; // Aligns with the target ViewModel or controller action.
        public string? UserId { get; set; } // Future: drive personalization and RBAC-aware filtering.
        public string? ClinicId { get; set; } // Future: constrain presets to clinic/department.
        public Dictionary<string, string[]> ActiveCriteria { get; set; } = new(); // Resolved criteria after preset merge + ad-hoc filters.
    }

    /*
     * Future persistence strategy:
     * - Persist presets via EF Core in a dedicated table (e.g., FilterPresets) keyed by tenant/user.
     * - Store Criteria as JSON for flexible field support; add indexed columns for common filters (date ranges, risk flags).
     * - Server-side application: translate Criteria into LINQ expressions per scope (Patients/Appointments) using a field registry.
     * - Support clinic-level defaults and user overrides by layering presets (system -> clinic -> user).
     * Razor hook points (no markup yet):
     * - Patients/Index and Appointments/Index: inject FilterPresetViewModel into the page ViewModel to hydrate filter drawers.
     * - Shared partial for preset selection + save actions, receiving the current FilterPresetApplicationContext.
     */
}
