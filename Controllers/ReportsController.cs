// File: Controllers/ReportsController.cs
using System;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.Reports;

namespace MedicalOfficeManagement.Controllers
{
    public class ReportsController : Controller
    {
        public ActionResult Index()
        {
            var model = new ReportsIndexViewModel
            {
                AppointmentsToday = 12,
                AppointmentsThisWeek = 84,
                AppointmentsThisMonth = 347,
                RevenueToday = 1840.00m,
                RevenueThisWeek = 12450.00m,
                RevenueThisMonth = 52890.00m,
                NewPatientsToday = 3,
                NewPatientsThisWeek = 18,
                NewPatientsThisMonth = 67,
                AverageWaitTime = "12 min"
            };

            return View(model);
        }
    }
}