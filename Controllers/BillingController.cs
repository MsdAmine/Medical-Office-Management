// File: Controllers/BillingController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.ViewModels.Billing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    public class BillingController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public BillingController(MedicalOfficeContext context)
        {
            _context = context;
        }

        public async Task<ActionResult> Index()
        {
            var consultations = await _context.Consultations
                .Include(c => c.Patient)
                .OrderByDescending(c => c.DateConsult)
                .ToListAsync();

            var invoices = consultations.Select(c =>
            {
                var status = DetermineStatus(c.DateConsult);
                return new InvoiceViewModel
                {
                    Id = c.Id,
                    PatientName = $"{c.Patient.Prenom} {c.Patient.Nom}",
                    Amount = CalculateAmount(c),
                    Status = status,
                    StatusColor = MapStatusColor(status),
                    DueDate = c.DateConsult.AddDays(30),
                    InvoiceDate = c.DateConsult
                };
            }).ToList();

            var model = new BillingIndexViewModel
            {
                Invoices = invoices,
                TotalDue = invoices.Where(i => i.Status is "Unpaid" or "Overdue").Sum(i => i.Amount),
                PaidThisMonth = invoices.Where(i => i.Status == "Paid" && i.InvoiceDate.Month == DateTime.Now.Month && i.InvoiceDate.Year == DateTime.Now.Year).Sum(i => i.Amount),
                OverdueCount = invoices.Count(i => i.Status == "Overdue")
            };

            return View(model);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        private static decimal CalculateAmount(Consultation consultation)
        {
            var baseAmount = 100m;
            if (!string.IsNullOrWhiteSpace(consultation.Diagnostics))
            {
                baseAmount += 50m;
            }

            if (!string.IsNullOrWhiteSpace(consultation.Observations))
            {
                baseAmount += 25m;
            }

            return baseAmount;
        }

        private static string DetermineStatus(DateTime invoiceDate)
        {
            var dueDate = invoiceDate.AddDays(30);
            if (invoiceDate < DateTime.Now.AddDays(-45))
            {
                return "Paid";
            }

            if (dueDate < DateTime.Now)
            {
                return "Overdue";
            }

            if (invoiceDate < DateTime.Now.AddDays(-7))
            {
                return "Unpaid";
            }

            return "Pending";
        }

        private static string MapStatusColor(string status)
        {
            return status switch
            {
                "Paid" => "bg-green-100 text-green-700 border-green-200",
                "Overdue" => "bg-red-100 text-red-700 border-red-200",
                "Unpaid" => "bg-amber-100 text-amber-700 border-amber-200",
                "Pending" => "bg-blue-100 text-blue-700 border-blue-200",
                _ => "bg-gray-100 text-gray-700 border-gray-200"
            };
        }
    }
}
