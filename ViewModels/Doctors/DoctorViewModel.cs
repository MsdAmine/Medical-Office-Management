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
        public string Role { get; set; } = "MD";
        public string Location { get; set; } = string.Empty;
        public string Language { get; set; } = "English";
        public string NextAvailableSlot { get; set; } = "Today";
        public string FirstAppointment { get; set; } = "—";
        public string LastAppointment { get; set; } = "—";
    }
}
