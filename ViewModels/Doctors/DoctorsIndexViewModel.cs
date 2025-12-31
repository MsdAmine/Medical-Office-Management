// File: ViewModels/Doctors/DoctorsIndexViewModel.cs
using System.Collections.Generic;
using MedicalOfficeManagement.ViewModels.Heatmaps;

namespace MedicalOfficeManagement.ViewModels.Doctors
{
    public class DoctorsIndexViewModel
    {
        public List<DoctorViewModel> Doctors { get; set; } = new();
        public int TotalDoctors { get; set; }
        public int OnDutyToday { get; set; }
        public HeatmapViewModel? WorkloadHeatmap { get; set; }
        public int SelectedBucketMinutes { get; set; } = 30;
        public int StartHour { get; set; } = 8;
        public int EndHour { get; set; } = 18;
    }
}
