// File: Controllers/DoctorsController.cs
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.Doctors;

namespace MedicalOfficeManagement.Controllers
{
    public class DoctorsController : Controller
    {
        public ActionResult Index()
        {
            var model = new DoctorsIndexViewModel
            {
                Doctors = new List<DoctorViewModel>
                {
                    new DoctorViewModel
                    {
                        Id = 1,
                        FullName = "Dr. Sarah Martinez",
                        Specialty = "Cardiology",
                        Phone = "(555) 100-2001",
                        Email = "s.martinez@clinic.com",
                        IsAvailable = true,
                        PatientsToday = 8,
                        ColorClass = "bg-blue-50 border-blue-200"
                    },
                    new DoctorViewModel
                    {
                        Id = 2,
                        FullName = "Dr. Michael Williams",
                        Specialty = "General Practice",
                        Phone = "(555) 100-2002",
                        Email = "m.williams@clinic.com",
                        IsAvailable = true,
                        PatientsToday = 12,
                        ColorClass = "bg-green-50 border-green-200"
                    },
                    new DoctorViewModel
                    {
                        Id = 3,
                        FullName = "Dr. Emily Chen",
                        Specialty = "Pediatrics",
                        Phone = "(555) 100-2003",
                        Email = "e.chen@clinic.com",
                        IsAvailable = false,
                        PatientsToday = 0,
                        ColorClass = "bg-purple-50 border-purple-200"
                    },
                    new DoctorViewModel
                    {
                        Id = 4,
                        FullName = "Dr. James Thompson",
                        Specialty = "Orthopedics",
                        Phone = "(555) 100-2004",
                        Email = "j.thompson@clinic.com",
                        IsAvailable = true,
                        PatientsToday = 6,
                        ColorClass = "bg-amber-50 border-amber-200"
                    },
                    new DoctorViewModel
                    {
                        Id = 5,
                        FullName = "Dr. Lisa Anderson",
                        Specialty = "Dermatology",
                        Phone = "(555) 100-2005",
                        Email = "l.anderson@clinic.com",
                        IsAvailable = true,
                        PatientsToday = 9,
                        ColorClass = "bg-teal-50 border-teal-200"
                    },
                    new DoctorViewModel
                    {
                        Id = 6,
                        FullName = "Dr. Robert Garcia",
                        Specialty = "Neurology",
                        Phone = "(555) 100-2006",
                        Email = "r.garcia@clinic.com",
                        IsAvailable = false,
                        PatientsToday = 0,
                        ColorClass = "bg-purple-50 border-purple-200"
                    }
                },
                TotalDoctors = 8,
                OnDutyToday = 6
            };
            
            return View(model);
        }

        public ActionResult Details(int id)
        {
            return View();
        }
    }
}