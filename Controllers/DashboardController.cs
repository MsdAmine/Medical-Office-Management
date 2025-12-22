// File: Controllers/DashboardController.cs
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MedicalOfficeManagement.ViewModels.Dashboard;

namespace MedicalOfficeManagement.Controllers
{
    public class DashboardController : Controller
    {
        public ActionResult Index()
        {
            var model = new DashboardViewModel
            {
                UserDisplayName = "Dr. Martinez",
                Now = DateTime.Now,
                
                Stats = new List<StatCardViewModel>
                {
                    new StatCardViewModel
                    {
                        Label = "Appointments Today",
                        Value = "12",
                        Icon = "calendar",
                        ColorClass = "bg-blue-50 text-blue-600 border-blue-200",
                        Url = Url.Action("Index", "Appointments")
                    },
                    new StatCardViewModel
                    {
                        Label = "Patients Waiting",
                        Value = "2",
                        Icon = "users",
                        ColorClass = "bg-green-50 text-green-600 border-green-200",
                        Url = Url.Action("Waiting", "Patients")
                    },
                    new StatCardViewModel
                    {
                        Label = "Lab Results",
                        Value = "5",
                        Icon = "flask",
                        ColorClass = "bg-amber-50 text-amber-600 border-amber-200",
                        Url = Url.Action("PendingResults", "Labs")
                    },
                    new StatCardViewModel
                    {
                        Label = "Messages",
                        Value = "7",
                        Icon = "mail",
                        ColorClass = "bg-purple-50 text-purple-600 border-purple-200",
                        Url = Url.Action("Inbox", "Messages")
                    }
                },
                
                QuickActions = new List<QuickActionViewModel>
                {
                    new QuickActionViewModel
                    {
                        Label = "Register Patient",
                        Icon = "user-plus",
                        Url = Url.Action("Create", "Patients"),
                        ColorClass = "bg-blue-600 hover:bg-blue-700"
                    },
                    new QuickActionViewModel
                    {
                        Label = "Schedule Appointment",
                        Icon = "calendar-plus",
                        Url = Url.Action("Create", "Appointments"),
                        ColorClass = "bg-green-600 hover:bg-green-700"
                    },
                    new QuickActionViewModel
                    {
                        Label = "Check-In Patient",
                        Icon = "check-circle",
                        Url = Url.Action("CheckIn", "Patients"),
                        ColorClass = "bg-purple-600 hover:bg-purple-700"
                    },
                    new QuickActionViewModel
                    {
                        Label = "Search Records",
                        Icon = "search",
                        Url = Url.Action("Search", "MedicalRecords"),
                        ColorClass = "bg-gray-600 hover:bg-gray-700"
                    }
                },
                
                Alerts = new List<AlertViewModel>
                {
                    new AlertViewModel
                    {
                        Message = "3 patients overdue for follow-up",
                        Type = "warning",
                        Timestamp = DateTime.Now.AddHours(-2),
                        Url = Url.Action("OverdueFollowups", "Patients")
                    },
                    new AlertViewModel
                    {
                        Message = "Dr. Chen's schedule updated",
                        Type = "info",
                        Timestamp = DateTime.Now.AddHours(-1),
                        Url = Url.Action("Schedule", "Doctors", new { id = 5 })
                    },
                    new AlertViewModel
                    {
                        Message = "Prescription authorization needed - ID 847",
                        Type = "warning",
                        Timestamp = DateTime.Now.AddMinutes(-30),
                        Url = Url.Action("Details", "Prescriptions", new { id = 847 })
                    },
                    new AlertViewModel
                    {
                        Message = "Lab results ready for review (5)",
                        Type = "info",
                        Timestamp = DateTime.Now.AddMinutes(-15),
                        Url = Url.Action("PendingResults", "Labs")
                    }
                },
                
                Tasks = new List<TaskViewModel>
                {
                    new TaskViewModel
                    {
                        Category = "Pending Approvals",
                        Count = 7,
                        Url = Url.Action("Approvals", "Tasks"),
                        ColorClass = "bg-amber-50 border-amber-200 text-amber-700"
                    },
                    new TaskViewModel
                    {
                        Category = "Follow-Up Required",
                        Count = 12,
                        Url = Url.Action("Followups", "Tasks"),
                        ColorClass = "bg-blue-50 border-blue-200 text-blue-700"
                    },
                    new TaskViewModel
                    {
                        Category = "Billing Issues",
                        Count = 3,
                        Url = Url.Action("Issues", "Billing"),
                        ColorClass = "bg-red-50 border-red-200 text-red-700"
                    },
                    new TaskViewModel
                    {
                        Category = "Insurance Pending",
                        Count = 8,
                        Url = Url.Action("Pending", "Insurance"),
                        ColorClass = "bg-purple-50 border-purple-200 text-purple-700"
                    }
                },
                
                Modules = new List<ModuleViewModel>
                {
                    new ModuleViewModel
                    {
                        Name = "Patients",
                        Icon = "users",
                        Stat = "1,247 Active",
                        Url = Url.Action("Index", "Patients"),
                        ColorClass = "bg-blue-50 hover:bg-blue-100 border-blue-200",
                        Badge = null
                    },
                    new ModuleViewModel
                    {
                        Name = "Appointments",
                        Icon = "calendar",
                        Stat = "12 Today",
                        Url = Url.Action("Index", "Appointments"),
                        ColorClass = "bg-green-50 hover:bg-green-100 border-green-200",
                        Badge = null
                    },
                    new ModuleViewModel
                    {
                        Name = "Doctors",
                        Icon = "user-md",
                        Stat = "8 On Duty",
                        Url = Url.Action("Index", "Doctors"),
                        ColorClass = "bg-purple-50 hover:bg-purple-100 border-purple-200",
                        Badge = null
                    },
                    new ModuleViewModel
                    {
                        Name = "Billing",
                        Icon = "credit-card",
                        Stat = "$2,340 Due",
                        Url = Url.Action("Index", "Billing"),
                        ColorClass = "bg-amber-50 hover:bg-amber-100 border-amber-200",
                        Badge = "3 urgent"
                    },
                    new ModuleViewModel
                    {
                        Name = "Reports",
                        Icon = "chart-bar",
                        Stat = "View Stats",
                        Url = Url.Action("Index", "Reports"),
                        ColorClass = "bg-teal-50 hover:bg-teal-100 border-teal-200",
                        Badge = null
                    },
                    new ModuleViewModel
                    {
                        Name = "Settings",
                        Icon = "cog",
                        Stat = "Configure",
                        Url = Url.Action("Index", "Settings"),
                        ColorClass = "bg-gray-50 hover:bg-gray-100 border-gray-200",
                        Badge = null
                    }
                },
                
                Activity = new List<ActivityViewModel>
                {
                    new ActivityViewModel
                    {
                        Timestamp = DateTime.Now.AddMinutes(-15),
                        Description = "Dr. Chen completed appointment with Patient #8471",
                        Icon = "check",
                        Url = Url.Action("Details", "Appointments", new { id = 8471 })
                    },
                    new ActivityViewModel
                    {
                        Timestamp = DateTime.Now.AddMinutes(-45),
                        Description = "New patient registered: Sarah Johnson",
                        Icon = "user-plus",
                        Url = Url.Action("Details", "Patients", new { id = 1523 })
                    },
                    new ActivityViewModel
                    {
                        Timestamp = DateTime.Now.AddHours(-1),
                        Description = "Lab results uploaded for Patient #6234",
                        Icon = "flask",
                        Url = Url.Action("Details", "Labs", new { id = 6234 })
                    },
                    new ActivityViewModel
                    {
                        Timestamp = DateTime.Now.AddHours(-2),
                        Description = "Payment received: $240.00",
                        Icon = "dollar",
                        Url = Url.Action("Details", "Payments", new { id = 9921 })
                    },
                    new ActivityViewModel
                    {
                        Timestamp = DateTime.Now.AddHours(-3),
                        Description = "Dr. Williams updated patient notes",
                        Icon = "edit",
                        Url = Url.Action("Details", "MedicalRecords", new { id = 4421 })
                    }
                }
            };
            
            return View(model);
        }
    }
}