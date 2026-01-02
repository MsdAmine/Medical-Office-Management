using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class BillingController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var invoices = GetSampleInvoices();

            var viewModel = new BillingSummaryViewModel
            {
                OutstandingBalance = invoices.Where(i => i.Status == "Pending" || i.Status == "Overdue").Sum(i => i.Amount),
                PaidThisMonth = invoices.Where(i => i.Status == "Paid" && i.IssuedOn.Month == DateTime.Today.Month).Sum(i => i.Amount),
                DraftInvoices = invoices.Count(i => i.Status == "Draft"),
                Invoices = invoices
            };

            ViewData["Title"] = "Billing";
            ViewData["Breadcrumb"] = "Billing";

            return View(viewModel);
        }

        private static List<BillingInvoiceViewModel> GetSampleInvoices()
        {
            var today = DateTime.Today;

            return new List<BillingInvoiceViewModel>
            {
                new()
                {
                    InvoiceNumber = "INV-1024",
                    PatientName = "Alice Martin",
                    Service = "Annual Checkup",
                    Amount = 180.00m,
                    Status = "Paid",
                    IssuedOn = today.AddDays(-10),
                    DueDate = today.AddDays(-2),
                    PaymentMethod = "Visa"
                },
                new()
                {
                    InvoiceNumber = "INV-1025",
                    PatientName = "Marc Dupont",
                    Service = "Laboratory Tests",
                    Amount = 240.00m,
                    Status = "Pending",
                    IssuedOn = today.AddDays(-3),
                    DueDate = today.AddDays(12),
                    PaymentMethod = "Insurance"
                },
                new()
                {
                    InvoiceNumber = "INV-1026",
                    PatientName = "Fatima Zahra",
                    Service = "Orthopedic Consultation",
                    Amount = 320.00m,
                    Status = "Overdue",
                    IssuedOn = today.AddDays(-20),
                    DueDate = today.AddDays(-5),
                    PaymentMethod = "Bank Transfer"
                },
                new()
                {
                    InvoiceNumber = "INV-1027",
                    PatientName = "Julien Bernard",
                    Service = "Diabetes Follow-up",
                    Amount = 210.00m,
                    Status = "Draft",
                    IssuedOn = today,
                    DueDate = today.AddDays(14),
                    PaymentMethod = "Pending"
                }
            };
        }
    }
}
