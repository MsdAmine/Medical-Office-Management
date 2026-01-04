using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.Admin)]
    public class ReportsController : Controller
    {
        private readonly MedicalOfficeContext _context;

        public ReportsController(MedicalOfficeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var reports = await _context.ReportArtifacts
                .AsNoTracking()
                .OrderByDescending(r => r.GeneratedOn)
                .ToListAsync();

            var today = DateTime.Today;

            var viewModel = new ReportsIndexViewModel
            {
                ActiveReports = reports.Count(r => r.Status == "Ready"),
                ScheduledReports = reports.Count(r => r.Status == "Scheduled"),
                ExportsThisMonth = reports.Count(r => r.GeneratedOn.Month == today.Month && r.GeneratedOn.Year == today.Year),
                Reports = reports
            };

            ViewData["Title"] = "Reports & Analytics";
            ViewData["Breadcrumb"] = "Administration";

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ReportArtifact { GeneratedOn = DateTime.Today });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReportArtifact report)
        {
            if (!ModelState.IsValid)
            {
                return View(report);
            }

            _context.ReportArtifacts.Add(report);
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

            var report = await _context.ReportArtifacts
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var report = await _context.ReportArtifacts.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReportArtifact report)
        {
            if (id != report.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(report);
            }

            try
            {
                _context.Update(report);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(report.Id))
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

            var report = await _context.ReportArtifacts
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var report = await _context.ReportArtifacts.FindAsync(id);
            if (report != null)
            {
                _context.ReportArtifacts.Remove(report);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ReportExists(int id)
        {
            return _context.ReportArtifacts.Any(e => e.Id == id);
        }
    }
}
