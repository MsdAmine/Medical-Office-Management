using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Services;

namespace MedicalOfficeManagement.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IDashboardMetricsService _dashboardService;

    public HomeController(IDashboardMetricsService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var viewModel = await _dashboardService.GetDashboardViewModelAsync(User);
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
