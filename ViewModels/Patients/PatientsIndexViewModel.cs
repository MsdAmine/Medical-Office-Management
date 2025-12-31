// File: ViewModels/Patients/PatientsIndexViewModel.cs
using System.Collections.Generic;
using MedicalOfficeManagement.ViewModels.Filters;

namespace MedicalOfficeManagement.ViewModels.Patients
{
    public class PatientsIndexViewModel
    {
        public List<PatientViewModel> Patients { get; set; } = new();
        public int TotalPatients { get; set; }
        public int ActivePatients { get; set; }
        public int NewThisMonth { get; set; }
        public FilterContextViewModel<PatientsFilterCriteria> FilterContext { get; set; } = default!;
        public List<string> DoctorOptions { get; set; } = new();
        public List<string> RiskLevels { get; set; } = new();
    }
}
