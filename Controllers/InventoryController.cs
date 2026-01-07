using MedicalOfficeManagement.Models;
using MedicalOfficeManagement.Models.Security;
using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize(Roles = SystemRoles.Admin)]
    public class InventoryController : Controller
    {
        private readonly MedicalOfficeContext _context;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(MedicalOfficeContext context, ILogger<InventoryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _context.InventoryItems
                .AsNoTracking()
                .OrderBy(i => i.ItemName)
                .ToListAsync();

            var viewModel = new InventoryIndexViewModel
            {
                LowStockCount = items.Count(i => i.Quantity <= i.ReorderLevel),
                TotalSkus = items.Count,
                ReorderAlerts = items.Count(i => i.Status == "Reorder"),
                Items = items
            };

            ViewData["Title"] = "Inventory";
            ViewData["Breadcrumb"] = "Administration";

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new InventoryItem());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InventoryItem item)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            _context.InventoryItems.Add(item);
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

            var item = await _context.InventoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InventoryItem item)
        {
            if (id != item.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(item);
            }

            try
            {
                _context.Update(item);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InventoryItemExists(item.Id))
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

            var item = await _context.InventoryItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
            {
                return NotFound();
            }

            return View(item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                _logger.LogWarning("Attempted to delete non-existent inventory item with ID {ItemId}", id);
                return NotFound();
            }

            try
            {
                _context.InventoryItems.Remove(item);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Inventory item {ItemId} deleted successfully", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting inventory item {ItemId}", id);
                TempData["StatusMessage"] = "Error: Failed to delete inventory item. Please try again.";
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(Index));
        }

        private bool InventoryItemExists(int id)
        {
            return _context.InventoryItems.Any(e => e.Id == id);
        }
    }
}
