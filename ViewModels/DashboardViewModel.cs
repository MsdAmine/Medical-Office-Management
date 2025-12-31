// File: ViewModels/Dashboard/DashboardViewModel.cs
using System;
using System.Collections.Generic;
using MedicalOfficeManagement.ViewModels.Heatmaps;

namespace MedicalOfficeManagement.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public string UserDisplayName { get; set; } = string.Empty;
        public DateTime Now { get; set; }
        public List<StatCardViewModel> Stats { get; set; } = new();
        public List<QuickActionViewModel> QuickActions { get; set; } = new();
        public List<AlertViewModel> Alerts { get; set; } = new();
        public List<TaskViewModel> Tasks { get; set; } = new();
        public List<ModuleViewModel> Modules { get; set; } = new();
        public List<ActivityViewModel> Activity { get; set; } = new();
        public int UnreadMessages { get; set; }
        public string ClinicStatusLabel { get; set; } = "On schedule";
        public string ClinicStatusTone { get; set; } = "success";
        public HeatmapViewModel? ClinicHeatmap { get; set; }
        public int SelectedBucketMinutes { get; set; } = 30;
        public int StartHour { get; set; } = 8;
        public int EndHour { get; set; } = 18;
    }
}
