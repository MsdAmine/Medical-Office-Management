// File: ViewModels/MedicalRecords/MedicalRecordViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.MedicalRecords
{
    public class MedicalRecordViewModel
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public string DiagnosisSummary { get; set; }
        public string RecordType { get; set; }
    }
}