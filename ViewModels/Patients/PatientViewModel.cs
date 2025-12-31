// File: ViewModels/Patients/PatientViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Patients
{
    public class PatientViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime? LastVisit { get; set; }
        public string LastVisitRelative { get; set; } = string.Empty;
        public string PrimaryDoctor { get; set; } = string.Empty;
        public List<string> ClinicalFlags { get; set; } = new();
    }
}
