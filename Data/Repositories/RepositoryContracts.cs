// File: Data/Repositories/RepositoryContracts.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MedicalOfficeManagement.Data.Entities;
using MedicalOfficeManagement.Domain.Filters;

namespace MedicalOfficeManagement.Data.Repositories
{
    /// <summary>
    /// Future use: abstraction to decouple controllers from EF Core.
    /// </summary>
    public interface IPatientRepository
    {
        Task<IReadOnlyList<PatientEntity>> ListAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<PatientEntity>> ListWithAppointmentsAsync(CancellationToken cancellationToken);
        Task<PatientEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }

    public interface IAppointmentRepository
    {
        Task<IReadOnlyList<AppointmentEntity>> ListAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<AppointmentEntity>> ListRangeAsync(DateTime start, DateTime end, CancellationToken cancellationToken);
        Task<IReadOnlyList<AppointmentEntity>> ListForDateAsync(DateTime date, CancellationToken cancellationToken);
        Task<AppointmentEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    }

    public interface IDoctorRepository
    {
        Task<IReadOnlyList<DoctorEntity>> ListAsync(CancellationToken cancellationToken);
        Task<DoctorEntity?> GetByIdAsync(string id, CancellationToken cancellationToken);
    }

    public interface IInvoiceRepository
    {
        Task<IReadOnlyList<InvoiceEntity>> ListAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Future use: repository for saved filter presets stored in the domain layer.
    /// </summary>
    public interface IFilterPresetRepository
    {
        FilterPreset? GetDefault(string targetPage);
        IReadOnlyList<FilterPreset> GetPresets(string targetPage);
        FilterPreset Upsert(FilterPreset preset);
    }
}
