using MedicalOfficeManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalOfficeManagement.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            var items = new List<InventoryItemViewModel>
            {
                new()
                {
                    ItemName = "Surgical Masks",
                    Category = "PPE",
                    Quantity = 450,
                    ReorderLevel = 300,
                    Status = "In Stock"
                },
                new()
                {
                    ItemName = "Nitrile Gloves",
                    Category = "PPE",
                    Quantity = 180,
                    ReorderLevel = 250,
                    Status = "Low Stock"
                },
                new()
                {
                    ItemName = "IV Catheters",
                    Category = "Supplies",
                    Quantity = 75,
                    ReorderLevel = 100,
                    Status = "Reorder"
                },
                new()
                {
                    ItemName = "Blood Collection Tubes",
                    Category = "Laboratory",
                    Quantity = 320,
                    ReorderLevel = 200,
                    Status = "In Stock"
                }
            };

            var viewModel = new InventoryIndexViewModel
            {
                LowStockCount = items.Count(i => i.Status != "In Stock"),
                TotalSkus = 42,
                ReorderAlerts = items.Count(i => i.Status == "Reorder"),
                Items = items
            };

            ViewData["Title"] = "Inventory";
            ViewData["Breadcrumb"] = "Administration";

            return View(viewModel);
        }
    }
}
