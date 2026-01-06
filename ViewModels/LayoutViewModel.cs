using System;

namespace MedicalOfficeManagement.ViewModels
{
    public class LayoutViewModel
    {
        public int UpcomingAppointments { get; set; }

        public int TotalPatients { get; set; }

        public int OpenInvoices { get; set; }

        public int PendingLabResults { get; set; }

        public int PendingAppointmentApprovals { get; set; }
    }
}
