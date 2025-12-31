using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Heatmaps
{
    public class HeatmapViewModel
    {
        public DateTime Date { get; set; }
        public int BucketMinutes { get; set; }
        public int StartHour { get; set; }
        public int EndHour { get; set; }
        public List<HeatmapRowViewModel> Rows { get; set; } = new();
        public List<HeatmapLegendItemViewModel> Legend { get; set; } = new();
    }

    public class HeatmapLegendItemViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Intensity { get; set; }
    }
}
