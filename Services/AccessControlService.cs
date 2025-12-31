// File: Services/AccessControlService.cs
using System;
using System.Linq;
using MedicalOfficeManagement.ViewModels.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace MedicalOfficeManagement.Services
{
    public interface IAccessControlService
    {
        AccessControlViewModel GetCurrent();
    }

    public class AccessControlService : IAccessControlService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public AccessControlService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public AccessControlViewModel GetCurrent()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var roleName = httpContext?.Request?.Headers["X-Demo-Role"].FirstOrDefault()
                           ?? httpContext?.Request?.Query["role"].FirstOrDefault()
                           ?? _configuration["AccessControl:DefaultRole"]
                           ?? nameof(AppRole.Admin);

            if (!Enum.TryParse<AppRole>(roleName, true, out var role))
            {
                role = AppRole.Admin;
            }

            var permissions = RolePermissions.For(role);

            return new AccessControlViewModel
            {
                CurrentRole = role,
                Permissions = permissions,
                CanViewPatients = permissions.HasFlag(PermissionFlag.CanCreatePatient) ||
                                  permissions.HasFlag(PermissionFlag.CanEditPatient),
                CanManageAppointments = permissions.HasFlag(PermissionFlag.CanScheduleAppointment),
                CanManageBilling = permissions.HasFlag(PermissionFlag.CanViewBilling) ||
                                   permissions.HasFlag(PermissionFlag.CanEditBilling),
                CanManageSettings = permissions.HasFlag(PermissionFlag.CanManageSettings),
                CanViewReports = permissions.HasFlag(PermissionFlag.CanViewReports)
            };
        }
    }
}
