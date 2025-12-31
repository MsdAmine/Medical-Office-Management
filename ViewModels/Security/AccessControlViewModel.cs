// File: ViewModels/Security/AccessControlViewModel.cs

namespace MedicalOfficeManagement.ViewModels.Security
{
    public class AccessControlViewModel
    {
        public AppRole CurrentRole { get; set; }
        public PermissionFlag Permissions { get; set; }

        public bool CanViewPatients { get; set; }
        public bool CanManageAppointments { get; set; }
        public bool CanManageBilling { get; set; }
        public bool CanManageSettings { get; set; }
        public bool CanViewReports { get; set; }

        public bool CanEditBilling => Permissions.HasFlag(PermissionFlag.CanEditBilling);
        public bool CanCreatePatients => Permissions.HasFlag(PermissionFlag.CanCreatePatient);
        public bool CanEditPatients => Permissions.HasFlag(PermissionFlag.CanEditPatient);
        public bool CanSchedule => Permissions.HasFlag(PermissionFlag.CanScheduleAppointment);
        public bool CanManageStaff => Permissions.HasFlag(PermissionFlag.CanManageStaff);

        public bool ShowBilling => Permissions.HasFlag(PermissionFlag.CanViewBilling);
        public bool ShowReports => Permissions.HasFlag(PermissionFlag.CanViewReports);

        public bool IsAdmin => CurrentRole == AppRole.Admin;
        public bool IsClinicalStaff => CurrentRole == AppRole.Physician || CurrentRole == AppRole.Nurse;

        public bool HasPermission(PermissionFlag flag) => Permissions.HasFlag(flag);
    }
}
