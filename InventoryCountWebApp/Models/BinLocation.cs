using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InventoryCountWebApp.Models
{
    public class BinLocation
    {
        public string Bin { get; set; }
        public bool ContainsOnlyNonZeros { get; set; }
        public bool ContainsOnlyCountedItems { get; set; }
        public bool ContainsUncountedOnHand { get; set; }
    }


}