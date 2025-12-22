// File: ViewModels/Dashboard/ActivityViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Dashboard
{
    public class ActivityViewModel
    {
        public DateTime Timestamp { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }
    }
}