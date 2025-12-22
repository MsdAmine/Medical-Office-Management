// File: ViewModels/Dashboard/DashboardViewModel.cs
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public string UserDisplayName { get; set; }
        public DateTime Now { get; set; }
        public List<StatCardViewModel> Stats { get; set; }
        public List<QuickActionViewModel> QuickActions { get; set; }
        public List<AlertViewModel> Alerts { get; set; }
        public List<TaskViewModel> Tasks { get; set; }
        public List<ModuleViewModel> Modules { get; set; }
        public List<ActivityViewModel> Activity { get; set; }
    }
}