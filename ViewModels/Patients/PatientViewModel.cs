// File: ViewModels/Patients/PatientViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Patients
{
    public class PatientViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime LastVisit { get; set; }
        public string Status { get; set; }
        public string StatusColor { get; set; }
    }
}