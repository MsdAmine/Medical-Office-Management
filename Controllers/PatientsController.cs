// File: Controllers/PatientsController.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.Patients;

namespace MedicalOfficeManagement.Controllers
{
    public class PatientsController : Controller
    {
        public ActionResult Index()
        {
            var model = new PatientsIndexViewModel
            {
                Patients = new List<PatientViewModel>
                {
                    new PatientViewModel
                    {
                        Id = 1001,
                        FullName = "Sarah Johnson",
                        Phone = "(555) 123-4567",
                        Email = "sarah.j@email.com",
                        LastVisit = DateTime.Now.AddDays(-5),
                        Status = "Active",
                        StatusColor = "bg-green-100 text-green-700 border-green-200"
                    },
                    new PatientViewModel
                    {
                        Id = 1002,
                        FullName = "Michael Chen",
                        Phone = "(555) 234-5678",
                        Email = "m.chen@email.com",
                        LastVisit = DateTime.Now.AddDays(-12),
                        Status = "Active",
                        StatusColor = "bg-green-100 text-green-700 border-green-200"
                    },
                    new PatientViewModel
                    {
                        Id = 1003,
                        FullName = "Emma Williams",
                        Phone = "(555) 345-6789",
                        Email = "emma.w@email.com",
                        LastVisit = DateTime.Now.AddDays(-45),
                        Status = "Inactive",
                        StatusColor = "bg-gray-100 text-gray-700 border-gray-200"
                    },
                    new PatientViewModel
                    {
                        Id = 1004,
                        FullName = "James Martinez",
                        Phone = "(555) 456-7890",
                        Email = "j.martinez@email.com",
                        LastVisit = DateTime.Now.AddDays(-2),
                        Status = "Active",
                        StatusColor = "bg-green-100 text-green-700 border-green-200"
                    },
                    new PatientViewModel
                    {
                        Id = 1005,
                        FullName = "Olivia Brown",
                        Phone = "(555) 567-8901",
                        Email = "olivia.b@email.com",
                        LastVisit = DateTime.Now.AddDays(-8),
                        Status = "Active",
                        StatusColor = "bg-green-100 text-green-700 border-green-200"
                    },
                    new PatientViewModel
                    {
                        Id = 1006,
                        FullName = "David Lee",
                        Phone = "(555) 678-9012",
                        Email = "d.lee@email.com",
                        LastVisit = DateTime.Now.AddMonths(-3),
                        Status = "Pending",
                        StatusColor = "bg-amber-100 text-amber-700 border-amber-200"
                    }
                },
                TotalPatients = 1247,
                ActivePatients = 1189,
                NewThisMonth = 23
            };
            
            return View(model);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }
    }
}