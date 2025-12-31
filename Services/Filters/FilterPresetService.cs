// File: Services/Filters/FilterPresetService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MedicalOfficeManagement.ViewModels.Filters;

namespace MedicalOfficeManagement.Services.Filters
{
    public interface IFilterPresetRepository
    {
        IReadOnlyList<FilterPreset> GetPresets(FilterTargetPage targetPage);

        void Upsert(FilterPreset preset);
    }

    public class InMemoryFilterPresetRepository : IFilterPresetRepository
    {
        private static readonly Dictionary<FilterTargetPage, List<FilterPreset>> Presets = new()
        {
            {
                FilterTargetPage.Patients,
                new List<FilterPreset>
                {
                    new()
                    {
                        PresetId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        Name = "All patients",
                        TargetPage = FilterTargetPage.Patients,
                        Criteria = new PatientsFilterCriteria(),
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        CreatedByRole = "Admin"
                    },
                    new()
                    {
                        PresetId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        Name = "Follow-ups due",
                        TargetPage = FilterTargetPage.Patients,
                        Criteria = new PatientsFilterCriteria { FollowUpDueOnly = true },
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        CreatedByRole = "Clinician"
                    }
                }
            },
            {
                FilterTargetPage.Appointments,
                new List<FilterPreset>
                {
                    new()
                    {
                        PresetId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                        Name = "All appointments",
                        TargetPage = FilterTargetPage.Appointments,
                        Criteria = new AppointmentsFilterCriteria(),
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        CreatedByRole = "Admin"
                    },
                    new()
                    {
                        PresetId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"),
                        Name = "Today - confirmed",
                        TargetPage = FilterTargetPage.Appointments,
                        Criteria = new AppointmentsFilterCriteria
                        {
                            Status = "Confirmed",
                            StartDate = DateTime.Today,
                            EndDate = DateTime.Today.AddDays(1).AddTicks(-1)
                        },
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        CreatedByRole = "FrontDesk"
                    }
                }
            }
        };

        public IReadOnlyList<FilterPreset> GetPresets(FilterTargetPage targetPage) =>
            Presets.TryGetValue(targetPage, out var list)
                ? list
                : Array.Empty<FilterPreset>();

        public void Upsert(FilterPreset preset)
        {
            if (!Presets.TryGetValue(preset.TargetPage, out var list))
            {
                list = new List<FilterPreset>();
                Presets[preset.TargetPage] = list;
            }

            var existingIndex = list.FindIndex(p => p.PresetId == preset.PresetId);
            if (existingIndex >= 0)
            {
                list[existingIndex] = preset;
                return;
            }

            list.Add(preset);
        }
    }

    public interface IFilterPresetService
    {
        FilterContextViewModel<TCriteria> BuildContext<TCriteria>(
            FilterTargetPage targetPage,
            TCriteria? inboundFilters,
            Guid? presetId,
            bool clearPreset,
            string? presetName = null,
            string? createdByRole = null,
            bool setAsDefault = false)
            where TCriteria : FilterCriteriaBase;

        FilterPreset SavePreset<TCriteria>(
            FilterTargetPage targetPage,
            string name,
            string createdByRole,
            TCriteria criteria,
            bool isDefault = false)
            where TCriteria : FilterCriteriaBase;
    }

    public class FilterPresetService : IFilterPresetService
    {
        private readonly IFilterPresetRepository _repository;

        public FilterPresetService(IFilterPresetRepository repository)
        {
            _repository = repository;
        }

        public FilterContextViewModel<TCriteria> BuildContext<TCriteria>(
            FilterTargetPage targetPage,
            TCriteria? inboundFilters,
            Guid? presetId,
            bool clearPreset,
            string? presetName = null,
            string? createdByRole = null,
            bool setAsDefault = false)
            where TCriteria : FilterCriteriaBase
        {
            inboundFilters ??= (TCriteria)Activator.CreateInstance(typeof(TCriteria))!;

            if (!string.IsNullOrWhiteSpace(presetName))
            {
                SavePreset(targetPage, presetName.Trim(), createdByRole ?? "Clinician", inboundFilters, setAsDefault);
            }

            var presets = _repository.GetPresets(targetPage).ToList();
            var appliedFilters = (TCriteria)inboundFilters.Clone();
            Guid? activePresetId = null;

            if (!clearPreset && presetId.HasValue)
            {
                var requestedPreset = presets.FirstOrDefault(p => p.PresetId == presetId.Value);
                if (requestedPreset?.Criteria is TCriteria typedCriteria)
                {
                    appliedFilters = (TCriteria)typedCriteria.Clone();
                    activePresetId = requestedPreset.PresetId;
                }
            }
            else if (!clearPreset && !inboundFilters.HasValues())
            {
                var defaultPreset = presets.FirstOrDefault(p => p.IsDefault);
                if (defaultPreset?.Criteria is TCriteria defaultCriteria)
                {
                    appliedFilters = (TCriteria)defaultCriteria.Clone();
                    activePresetId = defaultPreset.PresetId;
                }
            }

            if (!presetId.HasValue && inboundFilters.HasValues())
            {
                activePresetId = null;
            }

            return new FilterContextViewModel<TCriteria>
            {
                TargetPage = targetPage,
                CurrentFilters = appliedFilters,
                Presets = presets,
                ActivePresetId = activePresetId,
                CanSavePreset = true,
                CanEditPreset = true
            };
        }

        public FilterPreset SavePreset<TCriteria>(
            FilterTargetPage targetPage,
            string name,
            string createdByRole,
            TCriteria criteria,
            bool isDefault = false)
            where TCriteria : FilterCriteriaBase
        {
            var preset = new FilterPreset
            {
                Name = name,
                TargetPage = targetPage,
                Criteria = criteria.Clone(),
                IsDefault = isDefault,
                CreatedAt = DateTime.UtcNow,
                CreatedByRole = createdByRole
            };

            _repository.Upsert(preset);
            return preset;
        }
    }
}
