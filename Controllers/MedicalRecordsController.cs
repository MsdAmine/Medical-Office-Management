// File: Controllers/MedicalRecordsController.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.MedicalRecords;

namespace MedicalOfficeManagement.Controllers
{
    public class MedicalRecordsController : Controller
    {
        public ActionResult Index()
        {
            var model = new MedicalRecordsIndexViewModel
            {
                Records = new List<MedicalRecordViewModel>
                {
                    new MedicalRecordViewModel
                    {
                        Id = 701,
                        Date = DateTime.Now.AddDays(-2),
                        PatientName = "Sarah Johnson",
                        DoctorName = "Dr. Martinez",
                        DiagnosisSummary = "Annual physical examination - all vitals normal",
                        RecordType = "Checkup"
                    },
                    new MedicalRecordViewModel
                    {
                        Id = 702,
                        Date = DateTime.Now.AddDays(-5),
                        PatientName = "Michael Chen",
                        DoctorName = "Dr. Williams",
                        DiagnosisSummary = "Upper respiratory infection - prescribed antibiotics",
                        RecordType = "Visit"
                    },
                    new MedicalRecordViewModel
                    {
                        Id = 703,
                        Date = DateTime.Now.AddDays(-7),
                        PatientName = "Emma Williams",
                        DoctorName = "Dr. Chen",
                        DiagnosisSummary = "Lab results - blood work showing normal ranges",
                        RecordType = "Lab Results"
                    },
                    new MedicalRecordViewModel
                    {
                        Id = 704,
                        Date = DateTime.Now.AddDays(-10),
                        PatientName = "James Martinez",
                        DoctorName = "Dr. Martinez",
                        DiagnosisSummary = "Follow-up for hypertension - medication adjusted",
                        RecordType = "Follow-up"
                    },
                    new MedicalRecordViewModel
                    {
                        Id = 705,
                        Date = DateTime.Now.AddDays(-12),
                        PatientName = "Olivia Brown",
                        DoctorName = "Dr. Williams",
                        DiagnosisSummary = "Vaccination - flu shot administered",
                        RecordType = "Vaccination"
                    }
                },
                TotalRecords = 3421
            };

            return View(model);
        }

        public ActionResult Details(int id)
        {
            return View();
        }

        public ActionResult Search()
        {
            return View();
        }
    }
}