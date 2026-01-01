// File: Services/Filters/FilterPresetService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MedicalOfficeManagement.Data.Repositories;
using MedicalOfficeManagement.ViewModels.Filters;
using DomainFilterPreset = MedicalOfficeManagement.Domain.Filters.FilterPreset;
using UiFilterPreset = MedicalOfficeManagement.ViewModels.Filters.FilterPreset;

namespace MedicalOfficeManagement.Services.Filters
{
    public class InMemoryFilterPresetRepository : IFilterPresetRepository
    {
        private static readonly Dictionary<string, List<DomainFilterPreset>> Presets = new()
        {
            {
                FilterTargetPage.Patients.ToString(),
                new List<DomainFilterPreset>
                {
                    new() { Id = 1, Name = "All patients", TargetPage = FilterTargetPage.Patients.ToString(), IsDefault = true },
                    new() { Id = 2, Name = "Follow-ups due", TargetPage = FilterTargetPage.Patients.ToString(), IsDefault = false }
                }
            },
            {
                FilterTargetPage.Appointments.ToString(),
                new List<DomainFilterPreset>
                {
                    new() { Id = 3, Name = "All appointments", TargetPage = FilterTargetPage.Appointments.ToString(), IsDefault = true },
                    new() { Id = 4, Name = "Today - confirmed", TargetPage = FilterTargetPage.Appointments.ToString(), IsDefault = false }
                }
            }
        };

        private static int _nextId = 5;

        public DomainFilterPreset? GetDefault(string targetPage) =>
            Presets.TryGetValue(targetPage, out var list)
                ? list.FirstOrDefault(p => p.IsDefault)
                : null;

        public IReadOnlyList<DomainFilterPreset> GetPresets(string targetPage) =>
            Presets.TryGetValue(targetPage, out var list)
                ? list
                : Array.Empty<DomainFilterPreset>();

        public DomainFilterPreset Upsert(DomainFilterPreset preset)
        {
            if (!Presets.TryGetValue(preset.TargetPage, out var list))
            {
                list = new List<DomainFilterPreset>();
                Presets[preset.TargetPage] = list;
            }

            if (preset.Id == 0)
            {
                preset.Id = _nextId++;
                list.Add(preset);
                return preset;
            }

            var existingIndex = list.FindIndex(p => p.Id == preset.Id);
            if (existingIndex >= 0)
            {
                list[existingIndex] = preset;
                return preset;
            }

            list.Add(preset);
            return preset;
        }
    }

    public interface IFilterPresetService
    {
        FilterContextViewModel<TCriteria> BuildContext<TCriteria>(
            FilterTargetPage targetPage,
            TCriteria? inboundFilters,
            int? presetId,
            bool clearPreset,
            string? presetName = null,
            string? createdByRole = null,
            bool setAsDefault = false)
            where TCriteria : FilterCriteriaBase;

        UiFilterPreset SavePreset<TCriteria>(
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
        private static readonly Dictionary<int, PresetMetadata> PresetCriteria = new()
        {
            {
                1,
                new PresetMetadata(new PatientsFilterCriteria(), DateTime.UtcNow.AddDays(-10), "Admin")
            },
            {
                2,
                new PresetMetadata(new PatientsFilterCriteria { FollowUpDueOnly = true }, DateTime.UtcNow.AddDays(-5), "Clinician")
            },
            {
                3,
                new PresetMetadata(new AppointmentsFilterCriteria(), DateTime.UtcNow.AddDays(-10), "Admin")
            },
            {
                4,
                new PresetMetadata(
                    new AppointmentsFilterCriteria
                    {
                        Status = "Confirmed",
                        StartDate = DateTime.Today,
                        EndDate = DateTime.Today.AddDays(1).AddTicks(-1)
                    },
                    DateTime.UtcNow.AddDays(-2),
                    "FrontDesk")
            }
        };

        public FilterPresetService(IFilterPresetRepository repository)
        {
            _repository = repository;
        }

        public FilterContextViewModel<TCriteria> BuildContext<TCriteria>(
            FilterTargetPage targetPage,
            TCriteria? inboundFilters,
            int? presetId,
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

            var presets = _repository
                .GetPresets(targetPage.ToString())
                .Select(p => MapToViewModel<TCriteria>(p, targetPage))
                .ToList();

            var appliedFilters = (TCriteria)inboundFilters.Clone();
            int? activePresetId = null;

            if (!clearPreset && presetId.HasValue)
            {
                var requestedPreset = presets.FirstOrDefault(p => p.Id == presetId.Value);
                if (requestedPreset?.Criteria is TCriteria typedCriteria)
                {
                    appliedFilters = (TCriteria)typedCriteria.Clone();
                    activePresetId = requestedPreset.Id;
                }
            }
            else if (!clearPreset && !inboundFilters.HasValues())
            {
                var defaultPreset = presets.FirstOrDefault(p => p.IsDefault);
                if (defaultPreset?.Criteria is TCriteria defaultCriteria)
                {
                    appliedFilters = (TCriteria)defaultCriteria.Clone();
                    activePresetId = defaultPreset.Id;
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

        public UiFilterPreset SavePreset<TCriteria>(
            FilterTargetPage targetPage,
            string name,
            string createdByRole,
            TCriteria criteria,
            bool isDefault = false)
            where TCriteria : FilterCriteriaBase
        {
            var domainPreset = new DomainFilterPreset
            {
                Name = name,
                TargetPage = targetPage.ToString(),
                IsDefault = isDefault
            };

            var savedPreset = _repository.Upsert(domainPreset);
            PresetCriteria[savedPreset.Id] = new PresetMetadata(criteria.Clone(), DateTime.UtcNow, createdByRole);

            return MapToViewModel<TCriteria>(savedPreset, targetPage);
        }

        private static UiFilterPreset MapDomainPreset(DomainFilterPreset domainPreset, FilterTargetPage targetPage, FilterCriteriaBase criteria) =>
            new UiFilterPreset
            {
                Id = domainPreset.Id,
                Name = domainPreset.Name,
                TargetPage = targetPage,
                Criteria = criteria,
                IsDefault = domainPreset.IsDefault,
                CreatedAt = PresetCriteria.TryGetValue(domainPreset.Id, out var meta) ? meta.CreatedAt : DateTime.UtcNow,
                CreatedByRole = meta?.CreatedByRole ?? "System"
            };

        private static UiFilterPreset MapToViewModel<TCriteria>(DomainFilterPreset domainPreset, FilterTargetPage targetPage)
            where TCriteria : FilterCriteriaBase
        {
            var criteria = PresetCriteria.TryGetValue(domainPreset.Id, out var meta) && meta.Criteria is TCriteria storedCriteria
                ? (TCriteria)storedCriteria.Clone()
                : (TCriteria)Activator.CreateInstance(typeof(TCriteria))!;

            return MapDomainPreset(domainPreset, targetPage, criteria);
        }

        private record PresetMetadata(FilterCriteriaBase Criteria, DateTime CreatedAt, string CreatedByRole);
    }
}
