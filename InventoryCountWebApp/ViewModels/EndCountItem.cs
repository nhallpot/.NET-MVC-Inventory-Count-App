using InventoryCountWebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InventoryCountWebApp.ViewModels
{
    public class EndCountItem
    {
        public Item InventoryItem { get; set; }
        public bool OriginalQuantityChanged { get; set; }
    }
}