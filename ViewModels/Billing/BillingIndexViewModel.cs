// File: ViewModels/Billing/BillingIndexViewModel.cs
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels.Billing
{
    public class BillingIndexViewModel
    {
        public List<InvoiceViewModel> Invoices { get; set; } = new();
        public decimal TotalDue { get; set; }
        public decimal PaidThisMonth { get; set; }
        public int OverdueCount { get; set; }
    }
}
