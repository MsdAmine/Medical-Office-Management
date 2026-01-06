using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class LabResultViewModel
    {
        public int Id { get; set; }
        public string TestName { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CollectedOn { get; set; }
    }

    public class LabIndexViewModel
    {
        public int PendingResults { get; set; }
        public int CriticalFindings { get; set; }
        public int CompletedToday { get; set; }
        public IEnumerable<LabResultViewModel> Results { get; set; } = Array.Empty<LabResultViewModel>();
    }
}
