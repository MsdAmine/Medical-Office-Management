// File: Controllers/BillingController.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.Billing;

namespace MedicalOfficeManagement.Controllers
{
    public class BillingController : Controller
    {
        public ActionResult Index()
        {
            var model = new BillingIndexViewModel
            {
                Invoices = new List<InvoiceViewModel>
                {
                    new InvoiceViewModel
                    {
                        Id = 10001,
                        PatientName = "Sarah Johnson",
                        Amount = 245.00m,
                        Status = "Paid",
                        StatusColor = "bg-green-100 text-green-700 border-green-200",
                        DueDate = DateTime.Now.AddDays(-5),
                        InvoiceDate = DateTime.Now.AddDays(-20)
                    },
                    new InvoiceViewModel
                    {
                        Id = 10002,
                        PatientName = "Michael Chen",
                        Amount = 180.00m,
                        Status = "Unpaid",
                        StatusColor = "bg-amber-100 text-amber-700 border-amber-200",
                        DueDate = DateTime.Now.AddDays(10),
                        InvoiceDate = DateTime.Now.AddDays(-5)
                    },
                    new InvoiceViewModel
                    {
                        Id = 10003,
                        PatientName = "Emma Williams",
                        Amount = 420.00m,
                        Status = "Overdue",
                        StatusColor = "bg-red-100 text-red-700 border-red-200",
                        DueDate = DateTime.Now.AddDays(-15),
                        InvoiceDate = DateTime.Now.AddDays(-45)
                    },
                    new InvoiceViewModel
                    {
                        Id = 10004,
                        PatientName = "James Martinez",
                        Amount = 95.00m,
                        Status = "Paid",
                        StatusColor = "bg-green-100 text-green-700 border-green-200",
                        DueDate = DateTime.Now.AddDays(-2),
                        InvoiceDate = DateTime.Now.AddDays(-18)
                    },
                    new InvoiceViewModel
                    {
                        Id = 10005,
                        PatientName = "Olivia Brown",
                        Amount = 315.00m,
                        Status = "Unpaid",
                        StatusColor = "bg-amber-100 text-amber-700 border-amber-200",
                        DueDate = DateTime.Now.AddDays(5),
                        InvoiceDate = DateTime.Now.AddDays(-10)
                    }
                },
                TotalDue = 2340.00m,
                PaidThisMonth = 15670.00m,
                OverdueCount = 3
            };
            
            return View(model);
        }

        public ActionResult Details(int id)
        {
            return View();
        }
    }
}