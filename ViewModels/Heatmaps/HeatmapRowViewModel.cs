using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Heatmaps
{
    public class HeatmapRowViewModel
    {
        public string RowLabel { get; set; } = string.Empty;
        public List<HeatmapBucketViewModel> Buckets { get; set; } = new();
        public int TotalAppointments { get; set; }
        public double UtilizationPercent { get; set; }
        public string? NextAvailable { get; set; }
        public string? SummaryText { get; set; }
    }
}
