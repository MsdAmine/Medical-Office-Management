// File: ViewModels/Doctors/DoctorsIndexViewModel.cs
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Doctors
{
    public class DoctorsIndexViewModel
    {
        public List<DoctorViewModel> Doctors { get; set; }
        public int TotalDoctors { get; set; }
        public int OnDutyToday { get; set; }
    }
}