// File: Controllers/AppointmentsController.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.Appointments;

namespace MedicalOfficeManagement.Controllers
{
    public class AppointmentsController : Controller
    {
        public ActionResult Index()
        {
            var model = new AppointmentsIndexViewModel
            {
                Appointments = new List<AppointmentViewModel>
                {
                    new AppointmentViewModel
                    {
                        Id = 501,
                        Time = DateTime.Today.AddHours(9),
                        PatientName = "Sarah Johnson",
                        DoctorName = "Dr. Martinez",
                        Status = "Confirmed",
                        StatusColor = "bg-green-100 text-green-700 border-green-200"
                    },
                    new AppointmentViewModel
                    {
                        Id = 502,
                        Time = DateTime.Today.AddHours(10).AddMinutes(30),
                        PatientName = "Michael Chen",
                        DoctorName = "Dr. Williams",
                        Status = "Waiting",
                        StatusColor = "bg-blue-100 text-blue-700 border-blue-200"
                    },
                    new AppointmentViewModel
                    {
                        Id = 503,
                        Time = DateTime.Today.AddHours(11),
                        PatientName = "Emma Williams",
                        DoctorName = "Dr. Martinez",
                        Status = "Confirmed",
                        StatusColor = "bg-green-100 text-green-700 border-green-200"
                    },
                    new AppointmentViewModel
                    {
                        Id = 504,
                        Time = DateTime.Today.AddHours(13).AddMinutes(30),
                        PatientName = "James Martinez",
                        DoctorName = "Dr. Chen",
                        Status = "Completed",
                        StatusColor = "bg-gray-100 text-gray-700 border-gray-200"
                    },
                    new AppointmentViewModel
                    {
                        Id = 505,
                        Time = DateTime.Today.AddHours(14),
                        PatientName = "Olivia Brown",
                        DoctorName = "Dr. Williams",
                        Status = "Confirmed",
                        StatusColor = "bg-green-100 text-green-700 border-green-200"
                    },
                    new AppointmentViewModel
                    {
                        Id = 506,
                        Time = DateTime.Today.AddHours(15).AddMinutes(30),
                        PatientName = "David Lee",
                        DoctorName = "Dr. Martinez",
                        Status = "Cancelled",
                        StatusColor = "bg-red-100 text-red-700 border-red-200"
                    }
                },
                TodayCount = 12,
                ConfirmedCount = 8,
                WaitingCount = 2
            };
            
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }
    }
}