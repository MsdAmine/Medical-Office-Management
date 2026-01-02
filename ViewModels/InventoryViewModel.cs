using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class InventoryItemViewModel
    {
        public string ItemName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class InventoryIndexViewModel
    {
        public int LowStockCount { get; set; }
        public int TotalSkus { get; set; }
        public int ReorderAlerts { get; set; }
        public IEnumerable<InventoryItemViewModel> Items { get; set; } = Array.Empty<InventoryItemViewModel>();
    }
}
