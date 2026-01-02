using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class PrescriptionViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string Medication { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string PrescribedBy { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; }
        public DateTime? NextRefill { get; set; }
        public int RefillsRemaining { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class PrescriptionIndexViewModel
    {
        public int ActiveCount { get; set; }
        public int PendingCount { get; set; }
        public int CompletedCount { get; set; }
        public IEnumerable<PrescriptionViewModel> Prescriptions { get; set; } = Array.Empty<PrescriptionViewModel>();
    }
}
