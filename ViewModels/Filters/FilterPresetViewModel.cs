// File: ViewModels/Filters/FilterPresetViewModel.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalOfficeManagement.ViewModels.Filters
{
    public enum FilterTargetPage
    {
        Patients,
        Appointments
    }

    public abstract class FilterCriteriaBase
    {
        public abstract FilterTargetPage TargetPage { get; }

        public virtual bool HasValues() => false;

        public abstract FilterCriteriaBase Clone();
    }

    public class AppointmentsFilterCriteria : FilterCriteriaBase
    {
        public override FilterTargetPage TargetPage => FilterTargetPage.Appointments;

        public string? Doctor { get; set; }

        public string? Status { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public override bool HasValues() =>
            !string.IsNullOrWhiteSpace(Doctor) ||
            !string.IsNullOrWhiteSpace(Status) ||
            StartDate.HasValue ||
            EndDate.HasValue;

        public override FilterCriteriaBase Clone() => new AppointmentsFilterCriteria
        {
            Doctor = Doctor,
            Status = Status,
            StartDate = StartDate,
            EndDate = EndDate
        };
    }

    public class PatientsFilterCriteria : FilterCriteriaBase
    {
        public override FilterTargetPage TargetPage => FilterTargetPage.Patients;

        public string? PrimaryDoctor { get; set; }

        public string? RiskLevel { get; set; }

        public bool FollowUpDueOnly { get; set; }

        public override bool HasValues() =>
            !string.IsNullOrWhiteSpace(PrimaryDoctor) ||
            !string.IsNullOrWhiteSpace(RiskLevel) ||
            FollowUpDueOnly;

        public override FilterCriteriaBase Clone() => new PatientsFilterCriteria
        {
            PrimaryDoctor = PrimaryDoctor,
            RiskLevel = RiskLevel,
            FollowUpDueOnly = FollowUpDueOnly
        };
    }

    public class FilterPreset
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public FilterTargetPage TargetPage { get; set; }

        public FilterCriteriaBase Criteria { get; set; } = default!;

        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedByRole { get; set; } = string.Empty;
    }

    public interface IFilterContextViewModel
    {
        FilterTargetPage TargetPage { get; }

        int? ActivePresetId { get; }

        IReadOnlyList<FilterPreset> Presets { get; }

        FilterPreset? ActivePreset { get; }

        bool HasActivePreset { get; }

        bool IsPresetActive(int presetId);

        bool CanSavePreset { get; set; }

        bool CanEditPreset { get; set; }
    }

    public class FilterContextViewModel<TCriteria> : IFilterContextViewModel where TCriteria : FilterCriteriaBase
    {
        public FilterTargetPage TargetPage { get; set; }

        public TCriteria CurrentFilters { get; set; } = default!;

        public List<FilterPreset> Presets { get; set; } = new();

        public int? ActivePresetId { get; set; }

        public bool CanSavePreset { get; set; } = true;

        public bool CanEditPreset { get; set; } = true;

        public FilterPreset? ActivePreset => ActivePresetId.HasValue
            ? Presets.FirstOrDefault(p => p.Id == ActivePresetId.Value)
            : null;

        public bool HasActivePreset => ActivePresetId.HasValue;

        public bool IsPresetActive(int presetId) => ActivePresetId == presetId;

        IReadOnlyList<FilterPreset> IFilterContextViewModel.Presets => Presets;
    }

    public class PresetSelectorViewModel
    {
        public required IFilterContextViewModel Context { get; set; }

        public string ControllerName { get; set; } = string.Empty;

        public string ActionName { get; set; } = "Index";

        public string ManagePresetsHref { get; set; } = "#";
    }
}
