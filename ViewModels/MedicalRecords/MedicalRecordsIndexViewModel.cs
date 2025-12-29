// File: ViewModels/MedicalRecords/MedicalRecordsIndexViewModel.cs
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.MedicalRecords
{
    public class MedicalRecordsIndexViewModel
    {
        public List<MedicalRecordViewModel> Records { get; set; } = new();
        public int TotalRecords { get; set; }
    }
}
