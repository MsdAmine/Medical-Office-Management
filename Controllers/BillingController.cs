using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using MedicalOfficeManagement.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.AdminOrSecretaire)]
    public class BillingController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly ILogger<BillingController> _logger;
        private const int PageSize = 20;

        public BillingController(MedicalOfficeContext context, ILogger<BillingController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.BillingInvoices
                .AsNoTracking()
                .Include(i => i.Patient)
                .OrderByDescending(i => i.IssuedOn);

            var paginatedInvoices = await PaginatedList<BillingInvoice>.CreateAsync(query, page, PageSize);

            var today = DateTime.Today;
            var allInvoices = await _context.BillingInvoices.AsNoTracking().ToListAsync();

            var viewModel = new BillingSummaryViewModel
            {
                OutstandingBalance = allInvoices.Where(i => i.Status == "Pending" || i.Status == "Overdue").Sum(i => i.Amount),
                PaidThisMonth = allInvoices.Where(i => i.Status == "Paid" && i.IssuedOn.Month == today.Month && i.IssuedOn.Year == today.Year).Sum(i => i.Amount),
                DraftInvoices = allInvoices.Count(i => i.Status == "Draft"),
                Invoices = paginatedInvoices
            };

            ViewData["Title"] = "Billing";
            ViewData["Breadcrumb"] = "Billing";
            ViewData["PageIndex"] = paginatedInvoices.PageIndex;
            ViewData["TotalPages"] = paginatedInvoices.TotalPages;
            ViewData["HasPreviousPage"] = paginatedInvoices.HasPreviousPage;
            ViewData["HasNextPage"] = paginatedInvoices.HasNextPage;

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var invoice = new BillingInvoice
            {
                IssuedOn = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14)
            };

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillingInvoice invoice)
        {
            if (!ModelState.IsValid)
            {
                return View(invoice);
            }

            try
            {
                // Ensure PatientId is set if PatientName is provided
                if (invoice.PatientId <= 0 && !string.IsNullOrWhiteSpace(invoice.PatientName))
                {
                    var patient = await _context.Patients
                        .FirstOrDefaultAsync(p => (p.Nom + " " + p.Prenom).Trim() == invoice.PatientName.Trim());
                    if (patient != null)
                    {
                        invoice.PatientId = patient.Id;
                    }
                }

                _context.BillingInvoices.Add(invoice);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Billing invoice {InvoiceId} created successfully", invoice.Id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating billing invoice");
                ModelState.AddModelError(string.Empty, "An error occurred while creating the invoice. Please try again.");
                return View(invoice);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.BillingInvoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.BillingInvoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BillingInvoice invoice)
        {
            if (id != invoice.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(invoice);
            }

            try
            {
                _context.Update(invoice);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(invoice.Id))
                {
                    return NotFound();
                }

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.BillingInvoices
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.BillingInvoices.FindAsync(id);
            if (invoice == null)
            {
                _logger.LogWarning("Attempted to delete non-existent invoice with ID {InvoiceId}", id);
                return NotFound();
            }

            try
            {
                _context.BillingInvoices.Remove(invoice);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Invoice {InvoiceId} deleted successfully", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting invoice {InvoiceId}", id);
                TempData["StatusMessage"] = "Error: Failed to delete invoice. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InvoiceExists(int id)
        {
            return _context.BillingInvoices.Any(e => e.Id == id);
        }
    }
}
