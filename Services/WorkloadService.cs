using MedicalOfficeManagement.Data;
using MedicalOfficeManagement.Data.Repositories;
using MedicalOfficeManagement.ViewModels.Heatmaps;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace MedicalOfficeManagement.Services
{
    public class WorkloadService : IWorkloadService
    {
        private readonly MedicalOfficeDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public WorkloadService(MedicalOfficeDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public async Task<HeatmapViewModel> GetClinicHeatmapAsync(
            DateTime date,
            int bucketMinutes,
            int startHour,
            int endHour,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"clinic-heatmap-{date:yyyyMMdd}-{bucketMinutes}-{startHour}-{endHour}";
            if (_memoryCache.TryGetValue(cacheKey, out HeatmapViewModel? cached) && cached != null)
            {
                return cached;
            }

            var start = date.Date.AddHours(startHour);
            var end = date.Date.AddHours(endHour);
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.StartTime < end && a.EndTime > start)
                .ToListAsync(cancellationToken);

            var buckets = CreateBucketTemplate(start, end, bucketMinutes);
            var bucketLoads = new int[buckets.Count];
            var doctorPresence = Enumerable.Range(0, buckets.Count).Select(_ => new HashSet<string>()).ToList();
            var totalDoctorCount = await _context.Doctors.AsNoTracking().CountAsync(cancellationToken);

            foreach (var appt in appointments)
            {
                var apptStart = appt.StartTime < start ? start : appt.StartTime;
                var apptEnd = appt.EndTime > end ? end : appt.EndTime;
                if (apptEnd <= apptStart)
                {
                    continue;
                }

                var startIndex = GetBucketIndex(apptStart, start, bucketMinutes, buckets.Count);
                var endIndex = GetBucketIndex(apptEnd, start, bucketMinutes, buckets.Count, true);

                for (var i = startIndex; i < endIndex; i++)
                {
                    bucketLoads[i]++;
                    doctorPresence[i].Add(appt.DoctorId);
                }
            }

            var row = new HeatmapRowViewModel
            {
                RowLabel = "Clinic",
                Buckets = new List<HeatmapBucketViewModel>(),
            };

            var peakLabel = "—";
            var peakLoad = -1;
            double totalCapacity = 0;
            double totalLoad = 0;
            var activeDoctorCount = appointments.Select(a => a.DoctorId).Distinct().Count();

            for (var i = 0; i < buckets.Count; i++)
            {
                var capacity = doctorPresence[i].Count;
                if (capacity == 0)
                {
                    capacity = activeDoctorCount > 0 ? activeDoctorCount : Math.Max(1, totalDoctorCount);
                }

                var ratio = capacity == 0 ? 0 : (double)bucketLoads[i] / capacity;
                var intensity = CalculateIntensity(ratio);
                var bucket = buckets[i];
                row.Buckets.Add(new HeatmapBucketViewModel
                {
                    StartTime = bucket.StartTime,
                    EndTime = bucket.EndTime,
                    Label = bucket.Label,
                    LoadCount = bucketLoads[i],
                    LoadRatio = ratio,
                    Intensity = intensity,
                    Tooltip = $"{bucketLoads[i]} appointments, {(capacity == 0 ? 0 : ratio * 100):0}% capacity"
                });

                if (bucketLoads[i] > peakLoad)
                {
                    peakLoad = bucketLoads[i];
                    peakLabel = bucket.Label;
                }

                totalLoad += bucketLoads[i];
                totalCapacity += capacity;
            }

            row.TotalAppointments = (int)totalLoad;
            row.UtilizationPercent = totalCapacity == 0 ? 0 : (totalLoad / totalCapacity) * 100;
            row.SummaryText = $"Peak: {peakLabel}";

            var heatmap = new HeatmapViewModel
            {
                Date = date,
                BucketMinutes = bucketMinutes,
                StartHour = startHour,
                EndHour = endHour,
                Rows = new List<HeatmapRowViewModel> { row },
                Legend = BuildLegend()
            };

            _memoryCache.Set(cacheKey, heatmap, TimeSpan.FromMinutes(5));
            return heatmap;
        }

        public async Task<HeatmapViewModel> GetDoctorsHeatmapAsync(
            DateTime date,
            int bucketMinutes,
            int startHour,
            int endHour,
            CancellationToken cancellationToken)
        {
            var cacheKey = $"doctor-heatmap-{date:yyyyMMdd}-{bucketMinutes}-{startHour}-{endHour}";
            if (_memoryCache.TryGetValue(cacheKey, out HeatmapViewModel? cached) && cached != null)
            {
                return cached;
            }

            var start = date.Date.AddHours(startHour);
            var end = date.Date.AddHours(endHour);
            var doctors = await _context.Doctors.AsNoTracking().OrderBy(d => d.FullName).ToListAsync(cancellationToken);
            var appointments = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.StartTime < end && a.EndTime > start)
                .ToListAsync(cancellationToken);

            var buckets = CreateBucketTemplate(start, end, bucketMinutes);
            var rows = new List<HeatmapRowViewModel>();

            foreach (var doctor in doctors)
            {
                var doctorAppointments = appointments.Where(a => a.DoctorId == doctor.Id).ToList();
                var bucketLoads = new int[buckets.Count];

                foreach (var appt in doctorAppointments)
                {
                    var apptStart = appt.StartTime < start ? start : appt.StartTime;
                    var apptEnd = appt.EndTime > end ? end : appt.EndTime;
                    if (apptEnd <= apptStart)
                    {
                        continue;
                    }

                    var startIndex = GetBucketIndex(apptStart, start, bucketMinutes, buckets.Count);
                    var endIndex = GetBucketIndex(apptEnd, start, bucketMinutes, buckets.Count, true);

                    for (var i = startIndex; i < endIndex; i++)
                    {
                        bucketLoads[i]++;
                    }
                }

                var row = new HeatmapRowViewModel
                {
                    RowLabel = doctor.FullName,
                    Buckets = new List<HeatmapBucketViewModel>()
                };

                int totalAppointments = 0;
                var nextAvailableLabel = "—";
                for (var i = 0; i < buckets.Count; i++)
                {
                    var capacity = 1;
                    var ratio = (double)bucketLoads[i] / capacity;
                    var bucket = buckets[i];
                    row.Buckets.Add(new HeatmapBucketViewModel
                    {
                        StartTime = bucket.StartTime,
                        EndTime = bucket.EndTime,
                        Label = bucket.Label,
                        LoadCount = bucketLoads[i],
                        LoadRatio = ratio,
                        Intensity = CalculateIntensity(ratio),
                        Tooltip = $"{bucketLoads[i]} appointments, {(ratio * 100):0}% capacity"
                    });
                    totalAppointments += bucketLoads[i];

                    var isFutureBucket = date.Date > DateTime.Today ||
                        (date.Date == DateTime.Today && bucket.EndTime > DateTime.Now);
                    if (nextAvailableLabel == "—" && bucketLoads[i] == 0 && isFutureBucket)
                    {
                        nextAvailableLabel = bucket.Label;
                    }
                }

                var capacityTotal = buckets.Count;
                row.TotalAppointments = totalAppointments;
                row.UtilizationPercent = capacityTotal == 0 ? 0 : (double)totalAppointments / capacityTotal * 100;
                row.NextAvailable = nextAvailableLabel == "—" ? "Fully booked" : nextAvailableLabel;
                row.SummaryText = $"Utilization: {row.UtilizationPercent:0}%";

                rows.Add(row);
            }

            var heatmap = new HeatmapViewModel
            {
                Date = date,
                BucketMinutes = bucketMinutes,
                StartHour = startHour,
                EndHour = endHour,
                Rows = rows,
                Legend = BuildLegend()
            };

            _memoryCache.Set(cacheKey, heatmap, TimeSpan.FromMinutes(5));
            return heatmap;
        }

        private static int GetBucketIndex(DateTime time, DateTime windowStart, int bucketMinutes, int bucketCount, bool useCeiling = false)
        {
            var position = (time - windowStart).TotalMinutes / bucketMinutes;
            var index = useCeiling ? (int)Math.Ceiling(position) : (int)Math.Floor(position);
            if (index < 0) index = 0;
            if (index > bucketCount) index = bucketCount;
            return index;
        }

        private static List<HeatmapBucketViewModel> CreateBucketTemplate(DateTime start, DateTime end, int bucketMinutes)
        {
            var buckets = new List<HeatmapBucketViewModel>();
            for (var cursor = start; cursor < end; cursor = cursor.AddMinutes(bucketMinutes))
            {
                var bucketEnd = cursor.AddMinutes(bucketMinutes);
                buckets.Add(new HeatmapBucketViewModel
                {
                    StartTime = cursor,
                    EndTime = bucketEnd,
                    Label = cursor.ToString("HH:mm")
                });
            }

            return buckets;
        }

        private static int CalculateIntensity(double loadRatio)
        {
            if (loadRatio <= 0)
            {
                return 0;
            }

            if (loadRatio <= 0.25) return 1;
            if (loadRatio <= 0.5) return 2;
            if (loadRatio <= 0.75) return 3;
            return 4;
        }

        private static List<HeatmapLegendItemViewModel> BuildLegend()
        {
            return new List<HeatmapLegendItemViewModel>
            {
                new HeatmapLegendItemViewModel { Intensity = 0, Label = "0%", Description = "No load" },
                new HeatmapLegendItemViewModel { Intensity = 1, Label = "1-25%", Description = "Light" },
                new HeatmapLegendItemViewModel { Intensity = 2, Label = "26-50%", Description = "Moderate" },
                new HeatmapLegendItemViewModel { Intensity = 3, Label = "51-75%", Description = "Busy" },
                new HeatmapLegendItemViewModel { Intensity = 4, Label = "76-100%+", Description = "At capacity" }
            };
        }
    }
}
