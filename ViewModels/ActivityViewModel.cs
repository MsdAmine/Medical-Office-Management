// File: ViewModels/Dashboard/ActivityViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Dashboard
{
    public class ActivityViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
