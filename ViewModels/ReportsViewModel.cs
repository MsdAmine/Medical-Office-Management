using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class ReportViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Period { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime GeneratedOn { get; set; }
    }

    public class ReportsIndexViewModel
    {
        public int ActiveReports { get; set; }
        public int ScheduledReports { get; set; }
        public int ExportsThisMonth { get; set; }
        public IEnumerable<ReportViewModel> Reports { get; set; } = Array.Empty<ReportViewModel>();
    }
}
