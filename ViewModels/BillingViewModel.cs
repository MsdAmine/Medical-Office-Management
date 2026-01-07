using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Helpers;
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class BillingSummaryViewModel
    {
        public decimal OutstandingBalance { get; set; }
        public decimal PaidThisMonth { get; set; }
        public int DraftInvoices { get; set; }
        public PaginatedList<BillingInvoice> Invoices { get; set; } = null!;
    }
}
