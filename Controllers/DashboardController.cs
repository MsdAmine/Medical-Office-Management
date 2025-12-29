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
                        Description = "Capture demographics and insurance details",
                        Status = "Ready",
                        Icon = "user-plus",
                        Url = Url.Action("Create", "Patients"),
                        ColorClass = "bg-blue-600 hover:bg-blue-700"
                    },
                    new QuickActionViewModel
                    {
                        Label = "Schedule Appointment",
                        Description = "Book time with any available provider",
                        Status = "Next available",
                        Icon = "calendar-plus",
                        Url = Url.Action("Create", "Appointments"),
                        ColorClass = "bg-green-600 hover:bg-green-700"
                    },
                    new QuickActionViewModel
                    {
                        Label = "Check-In Patient",
                        Description = "Mark arrivals and update waiting room",
                        Status = "Front desk",
                        Icon = "check-circle",
                        Url = Url.Action("CheckIn", "Patients"),
                        ColorClass = "bg-purple-600 hover:bg-purple-700"
                    },
                    new QuickActionViewModel
                    {
                        Label = "Search Records",
                        Description = "Find clinical notes and lab results",
                        Status = "Recent",
                        Icon = "search",
                        Url = Url.Action("Search", "MedicalRecords"),
                        ColorClass = "bg-gray-600 hover:bg-gray-700"
                    }
                },
                
                Alerts = new List<AlertViewModel>
                {
                    new AlertViewModel
                    {
                        Title = "3 patients overdue for follow-up",
                        Description = "Schedule check-ins to keep care plans on track.",
                        Message = "3 patients overdue for follow-up",
                        Type = "warning",
                        Timestamp = DateTime.Now.AddHours(-2),
                        Url = Url.Action("OverdueFollowups", "Patients")
                    },
                    new AlertViewModel
                    {
                        Title = "Dr. Chen's schedule updated",
                        Description = "New availability opened later today.",
                        Message = "Dr. Chen's schedule updated",
                        Type = "info",
                        Timestamp = DateTime.Now.AddHours(-1),
                        Url = Url.Action("Schedule", "Doctors", new { id = 5 })
                    },
                    new AlertViewModel
                    {
                        Title = "Prescription authorization needed - ID 847",
                        Description = "Review the pending prescription for patient approval.",
                        Message = "Prescription authorization needed - ID 847",
                        Type = "warning",
                        Timestamp = DateTime.Now.AddMinutes(-30),
                        Url = Url.Action("Details", "Prescriptions", new { id = 847 })
                    },
                    new AlertViewModel
                    {
                        Title = "Lab results ready for review (5)",
                        Description = "Finalize and share results with patients.",
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
                        Title = "Pending Approvals",
                        Details = "7 requests need authorization",
                        Status = "Action required",
                        Category = "Pending Approvals",
                        Count = 7,
                        Url = Url.Action("Approvals", "Tasks"),
                        ColorClass = "bg-amber-50 border-amber-200 text-amber-700"
                    },
                    new TaskViewModel
                    {
                        Title = "Follow-Up Required",
                        Details = "12 patients awaiting outreach",
                        Status = "Open",
                        Category = "Follow-Up Required",
                        Count = 12,
                        Url = Url.Action("Followups", "Tasks"),
                        ColorClass = "bg-blue-50 border-blue-200 text-blue-700"
                    },
                    new TaskViewModel
                    {
                        Title = "Billing Issues",
                        Details = "3 claims flagged for review",
                        Status = "In review",
                        Category = "Billing Issues",
                        Count = 3,
                        Url = Url.Action("Issues", "Billing"),
                        ColorClass = "bg-red-50 border-red-200 text-red-700"
                    },
                    new TaskViewModel
                    {
                        Title = "Insurance Pending",
                        Details = "8 verifications in progress",
                        Status = "Pending",
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
                        Title = "Patients",
                        Description = "1,247 Active",
                        Icon = "users",
                        Url = Url.Action("Index", "Patients"),
                        ColorClass = "bg-blue-50 hover:bg-blue-100 border-blue-200",
                        Badge = string.Empty
                    },
                    new ModuleViewModel
                    {
                        Title = "Appointments",
                        Description = "12 Today",
                        Icon = "calendar",
                        Url = Url.Action("Index", "Appointments"),
                        ColorClass = "bg-green-50 hover:bg-green-100 border-green-200",
                        Badge = string.Empty
                    },
                    new ModuleViewModel
                    {
                        Title = "Doctors",
                        Description = "8 On Duty",
                        Icon = "user-md",
                        Url = Url.Action("Index", "Doctors"),
                        ColorClass = "bg-purple-50 hover:bg-purple-100 border-purple-200",
                        Badge = string.Empty
                    },
                    new ModuleViewModel
                    {
                        Title = "Billing",
                        Description = "$2,340 Due",
                        Icon = "credit-card",
                        Url = Url.Action("Index", "Billing"),
                        ColorClass = "bg-amber-50 hover:bg-amber-100 border-amber-200",
                        Badge = "3 urgent"
                    },
                    new ModuleViewModel
                    {
                        Title = "Reports",
                        Description = "View Stats",
                        Icon = "chart-bar",
                        Url = Url.Action("Index", "Reports"),
                        ColorClass = "bg-teal-50 hover:bg-teal-100 border-teal-200",
                        Badge = string.Empty
                    },
                    new ModuleViewModel
                    {
                        Title = "Settings",
                        Description = "Configure",
                        Icon = "cog",
                        Url = Url.Action("Index", "Settings"),
                        ColorClass = "bg-gray-50 hover:bg-gray-100 border-gray-200",
                        Badge = string.Empty
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
