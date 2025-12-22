// File: ViewModels/Patients/PatientsIndexViewModel.cs
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Patients
{
    public class PatientsIndexViewModel
    {
        public List<PatientViewModel> Patients { get; set; }
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int NewThisMonth { get; set; }
    }
}