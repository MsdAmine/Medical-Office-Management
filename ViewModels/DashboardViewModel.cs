// File: ViewModels/Dashboard/DashboardViewModel.cs
using System;
using System.Collections.Generic;

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
    }
}
