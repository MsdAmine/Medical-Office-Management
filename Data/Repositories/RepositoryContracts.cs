// File: Data/Repositories/RepositoryContracts.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MedicalOfficeManagement.Data.Entities;
using MedicalOfficeManagement.ViewModels.Filters;

namespace MedicalOfficeManagement.Data.Repositories
{
    /// <summary>
    /// Future use: abstraction to decouple controllers from EF Core.
    /// </summary>
    public interface IPatientRepository
    {
        Task<IReadOnlyList<PatientEntity>> ListAsync(CancellationToken cancellationToken);
    }

    public interface IAppointmentRepository
    {
        Task<IReadOnlyList<AppointmentEntity>> ListAsync(CancellationToken cancellationToken);
    }

    public interface IDoctorRepository
    {
        Task<IReadOnlyList<DoctorEntity>> ListAsync(CancellationToken cancellationToken);
    }

    public interface IInvoiceRepository
    {
        Task<IReadOnlyList<InvoiceEntity>> ListAsync(CancellationToken cancellationToken);
    }

    /// <summary>
    /// Future use: repository for saved filter presets with projection to ViewModels.
    /// </summary>
    public interface IFilterPresetRepository
    {
        Task<FilterPresetViewModel?> GetDefaultAsync(string scope, string userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<FilterPresetViewModel>> ListAsync(string scope, string userId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Future use: domain service for workload aggregation and severity banding.
    /// </summary>
    public interface IWorkloadService
    {
        Task<IReadOnlyList<ViewModels.Doctors.DoctorWorkloadViewModel>> GetDoctorWorkloadsAsync(
            DateTime start,
            DateTime end,
            int bucketMinutes,
            CancellationToken cancellationToken);
    }
}
