using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class ChartsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var charts = new List<ChartViewModel>
            {
                new()
                {
                    PatientName = "Alice Martin",
                    VisitReason = "Annual Physical",
                    Status = "In Review",
                    Provider = "Dr. Karim",
                    LastUpdated = DateTime.Now.AddHours(-3)
                },
                new()
                {
                    PatientName = "Marc Dupont",
                    VisitReason = "Diabetes Follow-up",
                    Status = "Signed",
                    Provider = "Dr. Chen",
                    LastUpdated = DateTime.Now.AddHours(-8)
                },
                new()
                {
                    PatientName = "Fatima Zahra",
                    VisitReason = "Post-op Check",
                    Status = "Awaiting Signature",
                    Provider = "Dr. Martin",
                    LastUpdated = DateTime.Now.AddHours(-12)
                },
                new()
                {
                    PatientName = "Julien Bernard",
                    VisitReason = "Cardiology Consult",
                    Status = "In Progress",
                    Provider = "Dr. Leroy",
                    LastUpdated = DateTime.Now.AddHours(-1)
                }
            };

            var viewModel = new ChartIndexViewModel
            {
                ActiveCharts = 42,
                PendingSignoffs = 6,
                CriticalAlerts = 2,
                Charts = charts
            };

            ViewData["Title"] = "Medical Charts";
            ViewData["Breadcrumb"] = "Clinical";

            return View(viewModel);
        }
    }
}
