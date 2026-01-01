// File: Services/IWorkloadService.cs
using MedicalOfficeManagement.ViewModels.Heatmaps;

namespace MedicalOfficeManagement.Services
{
    /// <summary>
    /// Domain service contract for workload aggregation and severity banding.
    /// </summary>
    public interface IWorkloadService
    {
        Task<HeatmapViewModel> GetClinicHeatmapAsync(
            DateTime date,
            int bucketMinutes,
            int startHour,
            int endHour,
            CancellationToken cancellationToken);

        Task<HeatmapViewModel> GetDoctorsHeatmapAsync(
            DateTime date,
            int bucketMinutes,
            int startHour,
            int endHour,
            CancellationToken cancellationToken);
    }
}
