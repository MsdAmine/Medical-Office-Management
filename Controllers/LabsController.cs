using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class LabsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var results = new List<LabResultViewModel>
            {
                new()
                {
                    TestName = "CBC with Differential",
                    PatientName = "Alice Martin",
                    Priority = "Routine",
                    Status = "Completed",
                    CollectedOn = DateTime.Today.AddHours(-2)
                },
                new()
                {
                    TestName = "Lipid Panel",
                    PatientName = "Marc Dupont",
                    Priority = "Routine",
                    Status = "In Progress",
                    CollectedOn = DateTime.Today.AddHours(-4)
                },
                new()
                {
                    TestName = "Troponin I",
                    PatientName = "Fatima Zahra",
                    Priority = "STAT",
                    Status = "Pending Review",
                    CollectedOn = DateTime.Today.AddHours(-1)
                },
                new()
                {
                    TestName = "HbA1c",
                    PatientName = "Julien Bernard",
                    Priority = "Routine",
                    Status = "Completed",
                    CollectedOn = DateTime.Today.AddHours(-6)
                }
            };

            var viewModel = new LabIndexViewModel
            {
                PendingResults = 5,
                CriticalFindings = 1,
                CompletedToday = 18,
                Results = results
            };

            ViewData["Title"] = "Lab Results";
            ViewData["Breadcrumb"] = "Clinical";

            return View(viewModel);
        }
    }
}
