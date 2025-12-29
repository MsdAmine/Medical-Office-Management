// File: ViewModels/Dashboard/AlertViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Dashboard
{
    public class AlertViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // "warning", "info", "error"
        public DateTime Timestamp { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
