using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class PrescriptionsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var prescriptions = GetSamplePrescriptions();

            var viewModel = new PrescriptionIndexViewModel
            {
                ActiveCount = prescriptions.Count(p => p.Status == "Active"),
                PendingCount = prescriptions.Count(p => p.Status == "Pending"),
                CompletedCount = prescriptions.Count(p => p.Status == "Completed"),
                Prescriptions = prescriptions
            };

            ViewData["Title"] = "Prescriptions";
            ViewData["Breadcrumb"] = "Prescriptions";

            return View(viewModel);
        }

        private static List<PrescriptionViewModel> GetSamplePrescriptions()
        {
            var today = DateTime.Today;

            return new List<PrescriptionViewModel>
            {
                new()
                {
                    Id = 1001,
                    PatientName = "Alice Martin",
                    Medication = "Amoxicillin",
                    Dosage = "500mg",
                    Frequency = "Twice daily",
                    PrescribedBy = "Dr. Leclerc",
                    IssuedOn = today.AddDays(-2),
                    NextRefill = today.AddDays(12),
                    RefillsRemaining = 2,
                    Status = "Active",
                    Notes = "Take with food"
                },
                new()
                {
                    Id = 1002,
                    PatientName = "Marc Dupont",
                    Medication = "Lisinopril",
                    Dosage = "10mg",
                    Frequency = "Once daily",
                    PrescribedBy = "Dr. Nguyen",
                    IssuedOn = today.AddDays(-14),
                    NextRefill = today.AddDays(16),
                    RefillsRemaining = 3,
                    Status = "Active",
                    Notes = "Monitor blood pressure weekly"
                },
                new()
                {
                    Id = 1003,
                    PatientName = "Fatima Zahra",
                    Medication = "Ibuprofen",
                    Dosage = "400mg",
                    Frequency = "As needed",
                    PrescribedBy = "Dr. Smith",
                    IssuedOn = today.AddDays(-5),
                    NextRefill = null,
                    RefillsRemaining = 0,
                    Status = "Completed",
                    Notes = "Use for post-procedure discomfort"
                },
                new()
                {
                    Id = 1004,
                    PatientName = "Julien Bernard",
                    Medication = "Metformin",
                    Dosage = "850mg",
                    Frequency = "Twice daily",
                    PrescribedBy = "Dr. Leclerc",
                    IssuedOn = today.AddDays(-1),
                    NextRefill = today.AddDays(29),
                    RefillsRemaining = 5,
                    Status = "Pending",
                    Notes = "Awaiting insurance authorization"
                }
            };
        }
    }
}
