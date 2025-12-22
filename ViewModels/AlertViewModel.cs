// File: ViewModels/Dashboard/AlertViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Dashboard
{
    public class AlertViewModel
    {
        public string Message { get; set; }
        public string Type { get; set; } // "warning", "info", "error"
        public DateTime Timestamp { get; set; }
        public string Url { get; set; }
    }
}