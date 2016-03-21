using System.Data.Entity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InventoryCountWebApp.Models
{
    public class Item
    {
        [Display(Name="Name")]
        public string Name { get; set; }

        [Display(Name="Bin")]
        public string BinLocation { get; set; }

        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "On Hand")]
        public int OnHand { get; set; }

        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name= "Cycle Count Code")]
        public int CycleCountCode { get; set; }

        [Required,Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Display(Name="Internal ID")]
        public int ID { get; set; }

        public string VendorCode { get; set; }

        public string Brand { get; set; }

        public int QuantityBackOrdered { get; set; }

        [Display(Name="Adjust Qty By")]
        public int NewOnHand { get; set; }

        [Display(Name="New Bin")]
        public string NewBinLocation { get; set; }

        public int WebProductID { get; set; }

        // An Item is confirmed if the on hand is the correct amount
        public bool Confirmed { get; set; }

        public string CounterInitials { get; set; }

        // A change is a change in amount on hand or if the user has entered in notes
        public bool ChangesMade
        {
            get
            {
                if (this.OnHand != NewOnHand || this.Notes != null)
                {
                    // Changes have been made
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool OnPartial { get; set; }

        public bool Counted
        {
            get
            {
                if(this.CounterInitials != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }

    public class ItemDBContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
    }
}