using System.Security.Claims;
using System.Threading.Tasks;
using MedicalOfficeManagement.ViewModels;

namespace MedicalOfficeManagement.Services;

public interface IDashboardMetricsService
{
    Task<LayoutViewModel> GetLayoutViewModelAsync(ClaimsPrincipal user);
    Task<DashboardViewModel> GetDashboardViewModelAsync(ClaimsPrincipal user);
}
