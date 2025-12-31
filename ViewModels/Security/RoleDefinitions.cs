// File: ViewModels/Security/RoleDefinitions.cs
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MedicalOfficeManagement.ViewModels.Security
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum AppRole
    {
        Admin,
        Physician,
        Nurse,
        FrontDesk,
        Billing
    }

    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PermissionFlag
    {
        None = 0,
        CanCreatePatient = 1 << 0,
        CanEditPatient = 1 << 1,
        CanScheduleAppointment = 1 << 2,
        CanViewBilling = 1 << 3,
        CanEditBilling = 1 << 4,
        CanViewReports = 1 << 5,
        CanManageStaff = 1 << 6,
        CanManageSettings = 1 << 7
    }

    public static class RolePermissions
    {
        private static readonly IReadOnlyDictionary<AppRole, PermissionFlag> RolePermissionMap =
            new Dictionary<AppRole, PermissionFlag>
            {
                [AppRole.Admin] = PermissionFlag.CanCreatePatient |
                                  PermissionFlag.CanEditPatient |
                                  PermissionFlag.CanScheduleAppointment |
                                  PermissionFlag.CanViewBilling |
                                  PermissionFlag.CanEditBilling |
                                  PermissionFlag.CanViewReports |
                                  PermissionFlag.CanManageStaff |
                                  PermissionFlag.CanManageSettings,

                [AppRole.Physician] = PermissionFlag.CanCreatePatient |
                                       PermissionFlag.CanEditPatient |
                                       PermissionFlag.CanScheduleAppointment |
                                       PermissionFlag.CanViewBilling |
                                       PermissionFlag.CanViewReports,

                [AppRole.Nurse] = PermissionFlag.CanCreatePatient |
                                  PermissionFlag.CanEditPatient |
                                  PermissionFlag.CanScheduleAppointment,

                [AppRole.FrontDesk] = PermissionFlag.CanCreatePatient |
                                       PermissionFlag.CanScheduleAppointment |
                                       PermissionFlag.CanViewBilling,

                [AppRole.Billing] = PermissionFlag.CanViewBilling |
                                     PermissionFlag.CanEditBilling |
                                     PermissionFlag.CanViewReports
            };

        public static PermissionFlag For(AppRole role)
        {
            return RolePermissionMap.TryGetValue(role, out var flags)
                ? flags
                : PermissionFlag.None;
        }
    }
}
