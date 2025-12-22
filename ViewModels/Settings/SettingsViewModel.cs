// File: ViewModels/Settings/SettingsViewModel.cs
namespace MedicalOfficeManagement.ViewModels.Settings
{
    public class SettingsViewModel
    {
        public string ClinicName { get; set; }
        public string ClinicAddress { get; set; }
        public string ClinicPhone { get; set; }
        public string ClinicEmail { get; set; }
        
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserRole { get; set; }
    }
}