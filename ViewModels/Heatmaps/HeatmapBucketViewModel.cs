using System;

namespace MedicalOfficeManagement.ViewModels.Heatmaps
{
    public class HeatmapBucketViewModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Label { get; set; } = string.Empty;
        public int LoadCount { get; set; }
        public double LoadRatio { get; set; }
        public int Intensity { get; set; }
        public string Tooltip { get; set; } = string.Empty;
    }
}
