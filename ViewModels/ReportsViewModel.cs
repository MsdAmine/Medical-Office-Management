using MedicalOfficeManagement.Models;
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class ReportsIndexViewModel
    {
        public int ActiveReports { get; set; }
        public int ScheduledReports { get; set; }
        public int ExportsThisMonth { get; set; }
        public IEnumerable<ReportArtifact> Reports { get; set; } = Array.Empty<ReportArtifact>();
    }
}
