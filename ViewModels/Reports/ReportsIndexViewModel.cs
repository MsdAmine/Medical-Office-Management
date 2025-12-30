// File: ViewModels/Reports/ReportsIndexViewModel.cs
namespace MedicalOfficeManagement.ViewModels.Reports
{
    public class ReportsIndexViewModel
    {
        public int AppointmentsToday { get; set; }
        public int AppointmentsThisWeek { get; set; }
        public int AppointmentsThisMonth { get; set; }

        public decimal RevenueToday { get; set; }
        public decimal RevenueThisWeek { get; set; }
        public decimal RevenueThisMonth { get; set; }

        public int NewPatientsToday { get; set; }
        public int NewPatientsThisWeek { get; set; }
        public int NewPatientsThisMonth { get; set; }

        public string AverageWaitTime { get; set; } = string.Empty;
    }
}
