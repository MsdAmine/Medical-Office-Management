// File: Services/AccessControlService.cs
using System;
using System.Linq;
using System.Security.Claims;
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
        private static readonly AppRole[] RolePriority =
        {
            AppRole.Admin,
            AppRole.Physician,
            AppRole.Nurse,
            AppRole.Billing,
            AppRole.Receptionist
        };

        public AccessControlService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        public AccessControlViewModel GetCurrent()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var resolvedRole = ResolveCurrentRole(httpContext?.User)
                               ?? httpContext?.Request?.Headers["X-Demo-Role"].FirstOrDefault()
                               ?? httpContext?.Request?.Query["role"].FirstOrDefault()
                               ?? _configuration["AccessControl:DefaultRole"]
                               ?? nameof(AppRole.Admin);

            if (!Enum.TryParse<AppRole>(resolvedRole, true, out var role))
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

        private string? ResolveCurrentRole(ClaimsPrincipal? user)
        {
            if (user?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            foreach (var role in RolePriority)
            {
                if (user.IsInRole(role.ToString()))
                {
                    return role.ToString();
                }
            }

            return user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }
    }
}
