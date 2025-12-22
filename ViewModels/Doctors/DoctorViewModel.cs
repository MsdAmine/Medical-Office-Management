// File: ViewModels/Doctors/DoctorViewModel.cs
namespace MedicalOfficeManagement.ViewModels.Doctors
{
    public class DoctorViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Specialty { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsAvailable { get; set; }
        public int PatientsToday { get; set; }
        public string ColorClass { get; set; }
    }
}