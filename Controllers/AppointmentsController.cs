using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var appointments = GetSampleAppointments();

            var viewModel = new ScheduleIndexViewModel
            {
                UpcomingCount = appointments.Count(a => a.StartTime >= DateTime.Today),
                InClinicToday = appointments.Count(a => a.StartTime.Date == DateTime.Today),
                CompletedThisWeek = appointments.Count(a => a.Status == "Completed"),
                Appointments = appointments
            };

            ViewData["Title"] = "Schedule";
            ViewData["Breadcrumb"] = "Schedule";

            return View(viewModel);
        }

        private static List<AppointmentViewModel> GetSampleAppointments()
        {
            var today = DateTime.Today;

            return new List<AppointmentViewModel>
            {
                new()
                {
                    Id = 301,
                    PatientName = "Alice Martin",
                    MedecinName = "Dr. Nguyen",
                    StartTime = today.AddHours(9),
                    EndTime = today.AddHours(9.5),
                    Location = "Exam Room 2",
                    Status = "Checked-in",
                    Reason = "Annual physical"
                },
                new()
                {
                    Id = 302,
                    PatientName = "Marc Dupont",
                    MedecinName = "Dr. Leclerc",
                    StartTime = today.AddHours(11),
                    EndTime = today.AddHours(11.5),
                    Location = "Exam Room 1",
                    Status = "Scheduled",
                    Reason = "Blood pressure follow-up"
                },
                new()
                {
                    Id = 303,
                    PatientName = "Fatima Zahra",
                    MedecinName = "Dr. Smith",
                    StartTime = today.AddDays(1).AddHours(10),
                    EndTime = today.AddDays(1).AddHours(10.5),
                    Location = "Telehealth",
                    Status = "Scheduled",
                    Reason = "Post-op check"
                },
                new()
                {
                    Id = 304,
                    PatientName = "Julien Bernard",
                    MedecinName = "Dr. Nguyen",
                    StartTime = today.AddDays(-1).AddHours(15),
                    EndTime = today.AddDays(-1).AddHours(15.5),
                    Location = "Exam Room 3",
                    Status = "Completed",
                    Reason = "Diabetes education"
                }
            };
        }
    }
}
