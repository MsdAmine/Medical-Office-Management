using MedicalOfficeManagement.Models;
using System;
using System.Collections.Generic;

namespace MedicalOfficeManagement.ViewModels
{
    public class InventoryIndexViewModel
    {
        public int LowStockCount { get; set; }
        public int TotalSkus { get; set; }
        public int ReorderAlerts { get; set; }
        public IEnumerable<InventoryItem> Items { get; set; } = Array.Empty<InventoryItem>();
    }
}
