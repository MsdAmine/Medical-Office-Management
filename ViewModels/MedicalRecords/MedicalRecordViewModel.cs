// File: ViewModels/MedicalRecords/MedicalRecordViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.MedicalRecords
{
    public class MedicalRecordViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string DiagnosisSummary { get; set; } = string.Empty;
        public string RecordType { get; set; } = string.Empty;
    }
}
