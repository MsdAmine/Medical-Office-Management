using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class ReportsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var reports = new List<ReportViewModel>
            {
                new()
                {
                    Title = "Monthly Revenue",
                    Period = "Last 30 days",
                    Owner = "Finance",
                    Status = "Scheduled",
                    GeneratedOn = DateTime.Today.AddDays(-1)
                },
                new()
                {
                    Title = "Patient Throughput",
                    Period = "This Week",
                    Owner = "Operations",
                    Status = "Ready",
                    GeneratedOn = DateTime.Today
                },
                new()
                {
                    Title = "Medication Usage",
                    Period = "Quarter to Date",
                    Owner = "Pharmacy",
                    Status = "In Progress",
                    GeneratedOn = DateTime.Today.AddDays(-3)
                },
                new()
                {
                    Title = "Denials & Appeals",
                    Period = "This Month",
                    Owner = "Billing",
                    Status = "Draft",
                    GeneratedOn = DateTime.Today.AddDays(-5)
                }
            };

            var viewModel = new ReportsIndexViewModel
            {
                ActiveReports = 12,
                ScheduledReports = 4,
                ExportsThisMonth = 18,
                Reports = reports
            };

            ViewData["Title"] = "Reports & Analytics";
            ViewData["Breadcrumb"] = "Administration";

            return View(viewModel);
        }
    }
}
