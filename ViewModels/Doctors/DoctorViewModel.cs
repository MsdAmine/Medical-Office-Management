// File: ViewModels/Doctors/DoctorViewModel.cs
namespace MedicalOfficeManagement.ViewModels.Doctors
{
    public class DoctorViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Specialty { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int PatientsToday { get; set; }
        public string ColorClass { get; set; } = string.Empty;
    }
}
