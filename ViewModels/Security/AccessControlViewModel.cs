// File: ViewModels/Security/AccessControlViewModel.cs

namespace MedicalOfficeManagement.ViewModels.Security
{
    /// <summary>
    /// Future use: surface RBAC results to Razor views without embedding authorization logic in markup.
    /// </summary>
    public class AccessControlViewModel
    {
        public bool CanViewPatients { get; set; }
        public bool CanManageAppointments { get; set; }
        public bool CanManageBilling { get; set; }
        public bool CanManageSettings { get; set; }
        public bool CanViewReports { get; set; }
    }
}
