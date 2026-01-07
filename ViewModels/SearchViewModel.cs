namespace MedicalOfficeManagement.ViewModels
{
    public class GlobalSearchViewModel
    {
        public string SearchTerm { get; set; } = string.Empty;
        public string SearchType { get; set; } = "all";
        public List<PatientSearchResult> Patients { get; set; } = new();
        public List<AppointmentSearchResult> Appointments { get; set; } = new();
    }

    public class PatientSearchResult
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public DateOnly? DateOfBirth { get; set; }
    }

    public class AppointmentSearchResult
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string Room { get; set; } = string.Empty;
    }
}
