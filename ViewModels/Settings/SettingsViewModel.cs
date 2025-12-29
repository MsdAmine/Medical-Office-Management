// File: ViewModels/Settings/SettingsViewModel.cs
namespace MedicalOfficeManagement.ViewModels.Settings
{
    public class SettingsViewModel
    {
        public string ClinicName { get; set; } = string.Empty;
        public string ClinicAddress { get; set; } = string.Empty;
        public string ClinicPhone { get; set; } = string.Empty;
        public string ClinicEmail { get; set; } = string.Empty;
        
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string UserRole { get; set; } = string.Empty;
    }
}
