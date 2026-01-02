using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class BillingInvoiceViewModel
    {
        public string InvoiceNumber { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime IssuedOn { get; set; }
        public DateTime DueDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class BillingSummaryViewModel
    {
        public decimal OutstandingBalance { get; set; }
        public decimal PaidThisMonth { get; set; }
        public int DraftInvoices { get; set; }
        public IEnumerable<BillingInvoiceViewModel> Invoices { get; set; } = Array.Empty<BillingInvoiceViewModel>();
    }
}
