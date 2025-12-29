// File: ViewModels/Billing/InvoiceViewModel.cs
using System;

namespace MedicalOfficeManagement.ViewModels.Billing
{
    public class InvoiceViewModel
    {
        public int Id { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusColor { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
}
