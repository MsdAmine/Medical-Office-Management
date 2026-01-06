using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class ChartViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string VisitReason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; }
    }

    public class ChartIndexViewModel
    {
        public int ActiveCharts { get; set; }
        public int PendingSignoffs { get; set; }
        public int CriticalAlerts { get; set; }
        public string? StatusMessage { get; set; }
        public IEnumerable<ChartViewModel> Charts { get; set; } = Array.Empty<ChartViewModel>();
    }
}
