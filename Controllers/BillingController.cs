using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.AdminOrSecretaire)]
    public class BillingController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public BillingController(MedicalOfficeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.BillingInvoices
                .AsNoTracking()
                .OrderByDescending(i => i.IssuedOn)
                .ToListAsync();

            var today = DateTime.Today;

            var viewModel = new BillingSummaryViewModel
            {
                OutstandingBalance = invoices.Where(i => i.Status == "Pending" || i.Status == "Overdue").Sum(i => i.Amount),
                PaidThisMonth = invoices.Where(i => i.Status == "Paid" && i.IssuedOn.Month == today.Month && i.IssuedOn.Year == today.Year).Sum(i => i.Amount),
                DraftInvoices = invoices.Count(i => i.Status == "Draft"),
                Invoices = invoices
            };

            ViewData["Title"] = "Billing";
            ViewData["Breadcrumb"] = "Billing";

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

            _context.BillingInvoices.Add(invoice);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
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
            if (invoice != null)
            {
                _context.BillingInvoices.Remove(invoice);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InvoiceExists(int id)
        {
            return _context.BillingInvoices.Any(e => e.Id == id);
        }
    }
}
