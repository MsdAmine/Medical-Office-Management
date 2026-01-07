using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using MedicalOfficeManagement.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using X.PagedList;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
            _logger.LogInformation("Billing Index accessed by user {UserId}, page {Page}", User.Identity?.Name, page);

            var query = _context.BillingInvoices
                .AsNoTracking()
                .Include(i => i.Patient)
                .OrderByDescending(i => i.IssuedOn);

            // Get total count
            var totalCount = await query.CountAsync();
            
            // Get items for current page
            var items = await query
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Create IPagedList manually
            var pagedInvoices = new StaticPagedList<BillingInvoice>(
                items,
                page,
                PageSize,
                totalCount);

            var today = DateTime.Today;
            var allInvoices = await _context.BillingInvoices.AsNoTracking().ToListAsync();

            var viewModel = new BillingSummaryViewModel
            {
                OutstandingBalance = allInvoices.Where(i => i.StatusEnum == InvoiceStatus.Pending || i.StatusEnum == InvoiceStatus.Overdue).Sum(i => i.Amount),
                PaidThisMonth = allInvoices.Where(i => i.StatusEnum == InvoiceStatus.Paid && i.IssuedOn.Month == today.Month && i.IssuedOn.Year == today.Year).Sum(i => i.Amount),
                DraftInvoices = allInvoices.Count(i => i.StatusEnum == InvoiceStatus.Draft),
                Invoices = pagedInvoices
            };

            ViewData["Title"] = "Billing";
            ViewData["Breadcrumb"] = "Billing";
            ViewData["PageNumber"] = pagedInvoices.PageNumber;
            ViewData["PageCount"] = pagedInvoices.PageCount;
            ViewData["TotalItemCount"] = pagedInvoices.TotalItemCount;
            ViewData["HasPreviousPage"] = pagedInvoices.HasPreviousPage;
            ViewData["HasNextPage"] = pagedInvoices.HasNextPage;

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? consultationId, int? patientId, string? service, decimal? amount)
        {
            var invoice = new BillingInvoice
            {
                IssuedOn = DateTime.Today,
                DueDate = DateTime.Today.AddDays(14),
                InvoiceNumber = GenerateInvoiceNumber()
            };

            // Pre-fill from consultation if provided
            if (consultationId.HasValue)
            {
                var consultation = await _context.Consultations
                    .Include(c => c.Patient)
                    .Include(c => c.RendezVous)
                    .FirstOrDefaultAsync(c => c.Id == consultationId.Value);

                if (consultation != null)
                {
                    invoice.ConsultationId = consultation.Id;
                    invoice.RendezVousId = consultation.RendezVousId;
                    invoice.PatientId = consultation.PatientId;
                    invoice.PatientName = consultation.Patient != null 
                        ? $"{consultation.Patient.Prenom} {consultation.Patient.Nom}".Trim() 
                        : "Unknown Patient";
                    invoice.Service = service ?? $"Consultation - {consultation.DateConsult:yyyy-MM-dd}";
                    invoice.Amount = amount ?? 0;
                }
            }
            else if (patientId.HasValue)
            {
                invoice.PatientId = patientId.Value;
                var patient = await _context.Patients.FindAsync(patientId.Value);
                if (patient != null)
                {
                    invoice.PatientName = $"{patient.Prenom} {patient.Nom}".Trim();
                }
                if (!string.IsNullOrWhiteSpace(service))
                {
                    invoice.Service = service;
                }
                if (amount.HasValue)
                {
                    invoice.Amount = amount.Value;
                }
            }

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BillingInvoice invoice)
        {
            _logger.LogInformation("Creating billing invoice for patient {PatientId} by user {UserId}", invoice.PatientId, User.Identity?.Name);

            // Ensure default status if not set
            if (string.IsNullOrWhiteSpace(invoice.Status))
            {
                invoice.StatusEnum = InvoiceStatus.Draft;
            }

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

                // Generate invoice number if not provided
                if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
                {
                    invoice.InvoiceNumber = GenerateInvoiceNumber();
                }

                _context.BillingInvoices.Add(invoice);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Billing invoice {InvoiceId} created successfully", invoice.Id);
                TempData["StatusMessage"] = "Invoice created successfully.";
                
                // Redirect to consultation details if created from consultation
                if (invoice.ConsultationId.HasValue)
                {
                    return RedirectToAction("Details", "Consultations", new { id = invoice.ConsultationId });
                }
                
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
                .Include(i => i.Patient)
                .Include(i => i.Consultation)
                .Include(i => i.RendezVous)
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
            _logger.LogInformation("Editing billing invoice {InvoiceId} by user {UserId}", id, User.Identity?.Name);

            if (id != invoice.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Billing invoice {InvoiceId} edit validation failed", id);
                return View(invoice);
            }

            try
            {
                _context.Update(invoice);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Billing invoice {InvoiceId} updated successfully", id);
                TempData["StatusMessage"] = "Invoice updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(invoice.Id))
                {
                    _logger.LogWarning("Billing invoice {InvoiceId} not found during edit", id);
                    return NotFound();
                }

                _logger.LogError("Concurrency exception while editing billing invoice {InvoiceId}", id);
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

        [HttpGet]
        public async Task<IActionResult> ExportCsv()
        {
            _logger.LogInformation("CSV export requested for billing invoices by user {UserId}", User.Identity?.Name);

            var invoices = await _context.BillingInvoices
                .AsNoTracking()
                .Include(i => i.Patient)
                .OrderByDescending(i => i.IssuedOn)
                .ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("Id,Invoice Number,Patient Name,Service,Amount,Status,Issued On,Due Date,Payment Method");

            foreach (var invoice in invoices)
            {
                var patientName = invoice.Patient != null ? $"{invoice.Patient.Prenom} {invoice.Patient.Nom}".Trim() : invoice.PatientName;
                var status = invoice.StatusEnum.ToString();

                builder.AppendLine($"{invoice.Id},\"{invoice.InvoiceNumber}\",\"{patientName}\",\"{invoice.Service}\",{invoice.Amount},\"{status}\",{invoice.IssuedOn:yyyy-MM-dd},{invoice.DueDate:yyyy-MM-dd},\"{invoice.PaymentMethod}\"");
            }

            var fileName = $"billing-invoices-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            _logger.LogInformation("CSV export completed. {Count} invoices exported", invoices.Count);

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDelete([FromForm] int[] invoiceIds)
        {
            _logger.LogInformation("Bulk delete requested for {Count} invoices by user {UserId}", invoiceIds?.Length ?? 0, User.Identity?.Name);

            if (invoiceIds == null || invoiceIds.Length == 0)
            {
                TempData["StatusMessage"] = "No invoices selected.";
                return RedirectToAction(nameof(Index));
            }

            var invoices = await _context.BillingInvoices
                .Where(i => invoiceIds.Contains(i.Id))
                .ToListAsync();

            if (!invoices.Any())
            {
                TempData["StatusMessage"] = "No valid invoices found to delete.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.BillingInvoices.RemoveRange(invoices);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully deleted {Count} invoices", invoices.Count);
                TempData["StatusMessage"] = $"{invoices.Count} invoice(s) deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk delete of invoices");
                TempData["StatusMessage"] = "Error: Failed to delete invoices. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateStatus([FromForm] int[] invoiceIds, [FromForm] string status)
        {
            _logger.LogInformation("Bulk status update requested for {Count} invoices to status {Status} by user {UserId}", 
                invoiceIds?.Length ?? 0, status, User.Identity?.Name);

            if (invoiceIds == null || invoiceIds.Length == 0)
            {
                TempData["StatusMessage"] = "No invoices selected.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(status) || !Enum.TryParse<InvoiceStatus>(status, true, out var statusEnum))
            {
                TempData["StatusMessage"] = "Invalid status provided.";
                return RedirectToAction(nameof(Index));
            }

            var invoices = await _context.BillingInvoices
                .Where(i => invoiceIds.Contains(i.Id))
                .ToListAsync();

            if (!invoices.Any())
            {
                TempData["StatusMessage"] = "No valid invoices found to update.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                foreach (var invoice in invoices)
                {
                    invoice.StatusEnum = statusEnum;
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Successfully updated {Count} invoices to status {Status}", invoices.Count, statusEnum);
                TempData["StatusMessage"] = $"{invoices.Count} invoice(s) updated to {statusEnum} successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during bulk status update of invoices");
                TempData["StatusMessage"] = "Error: Failed to update invoices. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkExport([FromForm] int[] invoiceIds)
        {
            _logger.LogInformation("Bulk CSV export requested for {Count} invoices by user {UserId}", invoiceIds?.Length ?? 0, User.Identity?.Name);

            if (invoiceIds == null || invoiceIds.Length == 0)
            {
                TempData["StatusMessage"] = "No invoices selected.";
                return RedirectToAction(nameof(Index));
            }

            var invoices = await _context.BillingInvoices
                .AsNoTracking()
                .Include(i => i.Patient)
                .Where(i => invoiceIds.Contains(i.Id))
                .OrderByDescending(i => i.IssuedOn)
                .ToListAsync();

            if (!invoices.Any())
            {
                TempData["StatusMessage"] = "No valid invoices found to export.";
                return RedirectToAction(nameof(Index));
            }

            var builder = new StringBuilder();
            builder.AppendLine("Id,Invoice Number,Patient Name,Service,Amount,Status,Issued On,Due Date,Payment Method");

            foreach (var invoice in invoices)
            {
                var patientName = invoice.Patient != null ? $"{invoice.Patient.Prenom} {invoice.Patient.Nom}".Trim() : invoice.PatientName;
                var status = invoice.StatusEnum.ToString();

                builder.AppendLine($"{invoice.Id},\"{invoice.InvoiceNumber}\",\"{patientName}\",\"{invoice.Service}\",{invoice.Amount},\"{status}\",{invoice.IssuedOn:yyyy-MM-dd},{invoice.DueDate:yyyy-MM-dd},\"{invoice.PaymentMethod}\"");
            }

            var fileName = $"billing-invoices-selected-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            _logger.LogInformation("Bulk CSV export completed. {Count} invoices exported", invoices.Count);

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPdf(int? id)
        {
            _logger.LogInformation("PDF export requested for invoice {InvoiceId} by user {UserId}", id, User.Identity?.Name);

            if (!id.HasValue)
            {
                TempData["StatusMessage"] = "Invoice ID is required.";
                return RedirectToAction(nameof(Index));
            }

            var invoice = await _context.BillingInvoices
                .AsNoTracking()
                .Include(i => i.Patient)
                .Include(i => i.Consultation)
                .Include(i => i.RendezVous)
                .FirstOrDefaultAsync(i => i.Id == id.Value);

            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for PDF export", id);
                return NotFound();
            }

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header()
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("Medical Office Management").FontSize(20).Bold();
                                column.Item().Text("Invoice").FontSize(16);
                            });

                            row.ConstantItem(100).AlignRight().Text($"Invoice #{invoice.InvoiceNumber}").FontSize(12);
                        });

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(column =>
                        {
                            column.Spacing(1, Unit.Centimetre);

                            // Patient Information
                            column.Item().PaddingBottom(0.5f, Unit.Centimetre).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                                .Column(col =>
                                {
                                    col.Item().Text("Bill To:").FontSize(12).Bold();
                                    col.Item().Text(invoice.PatientName).FontSize(11);
                                    if (invoice.Patient != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace(invoice.Patient.Adresse))
                                            col.Item().Text(invoice.Patient.Adresse).FontSize(10);
                                        if (!string.IsNullOrWhiteSpace(invoice.Patient.Email))
                                            col.Item().Text(invoice.Patient.Email).FontSize(10);
                                        if (!string.IsNullOrWhiteSpace(invoice.Patient.Telephone))
                                            col.Item().Text(invoice.Patient.Telephone).FontSize(10);
                                    }
                                });

                            // Invoice Details
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn();
                                });

                                table.Cell().Element(CellStyle).Text("Service:").Bold();
                                table.Cell().Element(CellStyle).Text(invoice.Service);

                                table.Cell().Element(CellStyle).Text("Issued On:").Bold();
                                table.Cell().Element(CellStyle).Text(invoice.IssuedOn.ToString("MMMM dd, yyyy"));

                                table.Cell().Element(CellStyle).Text("Due Date:").Bold();
                                table.Cell().Element(CellStyle).Text(invoice.DueDate.ToString("MMMM dd, yyyy"));

                                table.Cell().Element(CellStyle).Text("Status:").Bold();
                                table.Cell().Element(CellStyle).Text(invoice.Status);

                                table.Cell().Element(CellStyle).Text("Payment Method:").Bold();
                                table.Cell().Element(CellStyle).Text(invoice.PaymentMethod);
                            });

                            // Amount Section
                            column.Item().AlignRight().PaddingTop(1, Unit.Centimetre)
                                .Column(col =>
                                {
                                    col.Item().Row(row =>
                                    {
                                        row.ConstantItem(100).Text("Amount:").FontSize(12).Bold();
                                        row.RelativeItem().AlignRight().Text(invoice.Amount.ToString("C")).FontSize(14).Bold();
                                    });
                                });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text("Thank you for your business!")
                        .FontSize(10);
                });
            });

            var pdfBytes = document.GeneratePdf();
            var fileName = $"invoice-{invoice.InvoiceNumber}-{DateTime.Now:yyyyMMdd}.pdf";

            _logger.LogInformation("PDF export completed for invoice {InvoiceId}", id);
            return File(pdfBytes, "application/pdf", fileName);
        }

        static IContainer CellStyle(IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .PaddingVertical(5)
                .PaddingHorizontal(10);
        }

        private bool InvoiceExists(int id)
        {
            return _context.BillingInvoices.Any(e => e.Id == id);
        }

        private string GenerateInvoiceNumber()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month.ToString("D2");
            var day = DateTime.Now.Day.ToString("D2");
            var random = new Random().Next(1000, 9999);
            return $"INV-{year}{month}{day}-{random}";
        }
    }
}
