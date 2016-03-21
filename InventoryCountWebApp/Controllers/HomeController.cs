using Instrumart.SuiteTalk.WebService;
using InventoryCountWebApp.Database;
using InventoryCountWebApp.Models;
using InventoryCountWebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace InventoryCountWebApp.Controllers
{
    public class HomeController : Controller
    {
        private ItemDBContext db = new ItemDBContext();
        private NetSuiteService service { get; set; }
        private string email { get; set; }
        private string counterInitials { get; set; }
        private int adjustmentID { get; set; }
        private string password { get; set; }
        private bool NonWeeklyCount { get; set; }
        private static string[] AdminEmails = { "lhoffelt@instrumart.com", "noah@instrumart.com", "rpouliot@instrumart.com", "dgirouard@instrumart.com", "bmills@instrumart.com","bleffler@instrumart.com"};

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            this.service = filterContext.HttpContext.Session["NetSuiteService"] as NetSuiteService;
            this.email = filterContext.HttpContext.Session["Email"] as string;
            this.counterInitials = filterContext.HttpContext.Session["CounterInitials"] as string;
            this.password = filterContext.HttpContext.Session["Password"] as string;
            if(filterContext.HttpContext.Session["NonWeeklyCount"]!= null)
            {
                this.NonWeeklyCount = (bool)filterContext.HttpContext.Session["NonWeeklyCount"];
            }

            if(this.service==null || this.email==null || this.counterInitials ==null)
            {
                filterContext.Result = RedirectToAction("LogIn", "Account");
            }

            else
            {
                base.OnActionExecuting(filterContext);
            }
        }


        public ActionResult Index()
        {

            var items = from m in db.Items
                        select m;

            ViewBag.DBEmpty = "NotEmpty"; // Default the Database to not empty
            ViewBag.NonWeeklyCount = false;
            ViewBag.CanDelete = false;

            if (AdminEmails.Contains(this.email))
            {
                ViewBag.NonWeeklyCount = true;
                ViewBag.CanDelete = true;
            }

            int totalItems = items.ToList().Count();

            if(totalItems==0)
            {
                ViewBag.DBEmpty = "Empty"; // The Database is empty
            }
            
            var countFinished = SummaryOfCurrentCount();

            ViewBag.SummaryOfCurrentCount = countFinished; 

            var itemsNotCounted = countFinished[2];

            if(itemsNotCounted == 0)
            {
                ViewBag.CountFinished = "This count is finished as of the last item counted by: " + this.counterInitials; // Tag the last person who updated the items
            }

            return View();
        }

        public ActionResult BeginCount()
        {
            
            var allItems = from m in db.Items
                        select m;

            int count = allItems.Count();

            if (count == 0)
            {
                WeekForCount week = new WeekForCount();

                // Create a dictionary to populate select list in view
                Dictionary<string,string> dictionaryOfWeeks = new Dictionary<string,string>();

                // Add each week of the year to the dictionary
                for (int i = 1; i <= 52;i++)
                {
                    var firstDayOfWeek = DateRangeWeek.FirstDateOfWeek(DateTime.Now.Year, i);
                    dictionaryOfWeeks.Add(i.ToString(),"Week "+i +": "+firstDayOfWeek.ToShortDateString());
                }
                // Pass the dictionary into the View
                ViewBag.Weeks = dictionaryOfWeeks;

                    return View(week); 
            }
            else
            {
                return RedirectToAction("Index"); // Count in progress, return to home page
            }
            
        }

        [HttpPost]
        public ActionResult BeginCount(string confirm, WeekForCount week)
        {
            if (confirm.Equals("Begin Your New Count"))
            {
                NetSuiteService service = this.service;

                string email = this.email;

                string password = this.password;

                PopulateItemDB tableCreation = new PopulateItemDB();

                tableCreation.GenerateDB(week.WeekNumber,email,password);

            }
            return RedirectToAction("Index"); // Always return to index. 
        }

        public ActionResult Filter()
        {
            List<BinLocation> binLocations = ListOfBinLocations();

            ViewBag.BinLocations = binLocations;

            var items = from m in db.Items
                        select m;

            ViewBag.SummaryOfCurrentCount = SummaryOfCurrentCount();

            return View(items.OrderBy(x=>x.BinLocation).ThenBy(x=> x.Name));
        }

        [HttpPost]
        public ActionResult Filter(string nothing)
        {
            return RedirectToAction("Filter");
        }

        public ActionResult EndCount()
        {
            List<EndCountItem> endCountItems = new List<EndCountItem>();

            var items = from m in db.Items
                        select m;

            // Display only the items that have been changed in some way
            var itemsChanged = items.ToList().Where(x => x.ChangesMade || x.NewBinLocation!=x.BinLocation).OrderBy(x=>x.BinLocation);

            if(itemsChanged.Count()==0)
            {
                ViewBag.NoItemsChanged = "No items have changes on this count. Please delete this count on the Home Page if you are finished.";
            }

            foreach(var item in itemsChanged)
            {
                // If items on hand change since data was initially pulled, we want to update those 
                // on hand numbers and alert the user that they have changed

                EndCountItem endCountItem = new EndCountItem();

                var currentNetsuiteRecord = this.service.xGetRecord<InventoryItem>(item.ID.ToString());
                var locations = currentNetsuiteRecord.locationsList.locations;

                foreach (var location in locations)
                {
                    if (location.locationId.internalId == "1")
                    {
                        if (location.quantityOnHand != item.OnHand) // Quantity changed
                        {
                            item.OnHand = (int)location.quantityOnHand;
                            endCountItem.OriginalQuantityChanged = true;
                            endCountItem.InventoryItem = item;
                        }
                        else
                        {
                            endCountItem.InventoryItem = item;
                            endCountItem.OriginalQuantityChanged = false;
                        }
                        endCountItems.Add(endCountItem);
                    }
                }
            }

            return View(endCountItems);
        }

        [HttpPost]
        public ActionResult EndCount(string finishCount)
        {
            if (finishCount.Equals("Continue Working on Count"))
            {
                return RedirectToAction("Filter");
            }
            else
            {
                if (NoItemsLeftToCount()) // If there are no items left to count
                {
                    // Create the inventory adjustment
                    return RedirectToAction("InventoryAdjustment");
                }
                return RedirectToAction("Filter");
            }
        }

        public ActionResult InventoryAdjustment()
        {
            List<int> summmary = SummaryOfCurrentCount();

            if(summmary[2]!=0 || !AdminEmails.Contains(this.email)) // If there are items left to count, return them to the home page
            {
                return RedirectToAction("Index");
            }

            NetSuiteService service = this.service;

            // Create the adjustment object
            InventoryAdjustment adjustment = new InventoryAdjustment();

            adjustment.account = new RecordRef(RecordType.account, 147); // Set the adjustment account (147 is NetSuite ID)

            adjustment.adjLocation = new RecordRef(RecordType.location, 1); // Set adjustment location of South Burlington (1 is NetSuite ID)

            // All items
            var items = from m in db.Items
                        select m;

            // Determine the Week for the count
            // Take the one code that is greater than 39 (Class C items) and determine the week by subtracting 39 from that code
            int[] CycleCodes = items.Select(x => x.CycleCountCode).Distinct().ToArray();
            int week = 0;
            foreach (int cyclecode in CycleCodes)
            {
                if (cyclecode > 39)
                {
                    week = cyclecode - 39;
                }
            }
            if(this.NonWeeklyCount != true)
            {
                adjustment.memo = "cycle count " + week.ToString() + "--" + DateTime.Now.Year; // Set the adjustment memo
            }
            else
            {
                adjustment.memo = "entire column count " + DateTime.Now.Year;
            }

            // List of items that have been changed
            var itemsChanged = items.ToList().Where(x => x.OnHand != x.NewOnHand); // We only care about items that have different qty for Adjustments

            // List of items with bin location changes
            var binLocationChanged = items.ToList().Where(x => x.NewBinLocation != x.BinLocation);

            // Create a List of InventoryAdjustmentInventory
            InventoryAdjustmentInventoryList inventoryList = new InventoryAdjustmentInventoryList(); // Required for Adjustment

            inventoryList.inventory = new InventoryAdjustmentInventory[itemsChanged.Count()]; // An array of items to be changed

            int currentIndex = 0; // Keep track of the index

            // Change the Bin Location for items that have different Bins
            foreach (var item in binLocationChanged)
            {
                var id = item.ID.ToString();
                InventoryItem ii = new InventoryItem();
                ii.internalId = id;
                ii.xSetCustomField("custitem_bin_location", item.NewBinLocation.ToString());
                service.xUpdate(ii);
            }

            if (itemsChanged == null && binLocationChanged != null)
            {
                ViewBag.BinOnly = "You have only adjusted the Bin Location, no Inventory adjustment has been created.";
            }

            // Create Inventory adjustment for items that have different quantities
            if (itemsChanged.Count() != 0)
            {
                foreach (var item in itemsChanged) // Set the required fields for the Adjustment
                {
                    InventoryAdjustmentInventory iai = new InventoryAdjustmentInventory();
                    iai.item = new RecordRef((item.ID).ToString());
                    iai.location = new RecordRef("1");
                    iai.xSetAdjustQtyBy(item.NewOnHand - item.OnHand);
                    iai.memo = item.CounterInitials + ": " + item.Notes;
                    inventoryList.inventory[currentIndex] = iai; // Add to array
                    currentIndex++; // Update the Index
                }
                adjustment.inventoryList = inventoryList; // Required for adjustment

                var sumAdjusted = adjustment.inventoryList.inventory.Sum(x => x.adjustQtyBy); // Check to make sure that the local count and NetSuite count match

                WriteResponse adjustmentAdded = service.xAdd(adjustment); // Push to Netsuite



                if (adjustmentAdded.status.isSuccess) // Checks to make sure that the adjustment push was successfull
                {
                    // Create a copy Adjustment to check against the local copy
                    var id = adjustment.internalId;
                    var CountItemsChanged = itemsChanged.Count();
                    InventoryAdjustment ia = service.xGetRecord<InventoryAdjustment>(id);
                    var sumAdjustedCheck = ia.inventoryList.inventory.Sum(x => x.adjustQtyBy);

                    if (sumAdjusted == sumAdjustedCheck) // Makes sure the quantity changes are the same
                    {
                        ViewBag.adjustmentID = id;
                        return View(adjustmentAdded); // We are good to delete the count
                    }
                    else // The quantities are different
                    {
                        ViewBag.ErrorMessage("Something went wrong. Please check NetSuite and make sure everything is EXACTLY as you have entered in this software.");
                        return View(adjustmentAdded);// Need to check NetSuite, something went wrong

                    }
                }
                else // The NetSuite push was not successfull 
                {
                    ViewBag.ErrorMessage = adjustmentAdded.status.statusDetail[0].message.ToString();
                    return View(adjustmentAdded);

                }
            }
            // The user only changed the notes
            ViewBag.NotesOnly = "You did not change the quantity or bin location of any item in the count. Nothing has changed in NetSuite.";
            return View();

        }

        // Can't have a GET and POST having the same parameters, so string nothing is literally nothing
        [HttpPost]
        public ActionResult InventoryAdjustment(string nothing)
        {
            // Delete the database here, and then redirect to root
            DeleteDatabase();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteDB() 
        {
            if(AdminEmails.Contains(this.email))
            {
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public ActionResult DeleteDB(string confirmDelete)
        {
            if (confirmDelete.Equals("Delete Count") && AdminEmails.Contains(this.email))
            {
                // Delete the Database
                DeleteDatabase();
            }
            // Return to Index
            return RedirectToAction("Index");
        }

        // Info for one given item
        public ActionResult Description(int ID)
        {
            var items = from m in db.Items
                        where m.ID == ID 
                        select m;

            foreach(Item item in items)
            {
                if(item.ID == ID)
                {
                    return View(item);
                }
            }

            return View();
        }

        // Returns a list of all bin locations in the db
        public List<BinLocation> ListOfBinLocations()
        {
            IEnumerable<Item> Items = db.Items.ToList();

            List<BinLocation> BinLocations = new List<BinLocation>();

            IEnumerable<string> AllLocations = Items.Select(x => x.BinLocation).Distinct().OrderBy(x => x);

            // List of locations that have only items on hand
            string[] LocationsWithQtyOnHand = Items.Where(x => x.OnHand != 0 || x.NewOnHand != 0).Select(x => x.BinLocation).Distinct().ToArray();

            // List of locations that have only items that without changes
            string [] LocationsWithOnlyChanged = Items.Where(x => (x.ChangesMade) || (x.Confirmed)).Select(x => x.BinLocation).Distinct().ToArray();
            
            // Lis of locations that have changes and qty on hand
            string [] LocationsWithChangesAndOnHand = Items.Where(x=> (!(x.ChangesMade) && !(x.Confirmed))&&(x.OnHand!=0 || x.NewOnHand != 0)).Select(x=>x.BinLocation).Distinct().ToArray();

            foreach(string location in AllLocations)
            {
                BinLocation bin = new BinLocation();

                bin.Bin = location;
                bin.ContainsOnlyNonZeros = LocationsWithQtyOnHand.Contains(location);
                bin.ContainsOnlyCountedItems = LocationsWithOnlyChanged.Contains(location); // Change to LocationsWithChangesMade
                bin.ContainsUncountedOnHand = LocationsWithChangesAndOnHand.Contains(location);
                BinLocations.Add(bin);

            }
            // Add a blank bin for all bins
            BinLocation allBin = new BinLocation();
            allBin.Bin = "";
            BinLocations.Add(allBin);
            return BinLocations; 
        }

        public ActionResult UpdateDatabase(FormCollection form)
        {
            var items = from m in db.Items
                        select m;

            foreach (var item in items)
                    {
                // Confirmed the correct quantity
                        if (form[("ConfirmCheckForItemId" + item.ID)] != null && form[("ConfirmCheckForItemId" + item.ID)].Equals("ConfirmedCorrect"))
                        {
                            item.Confirmed = true;
                            item.NewOnHand = item.OnHand;
                            item.CounterInitials = this.counterInitials;
                        }
                // Updated the quantity
                        if ((form["NewQuantityForItemId" + item.ID] != "") && (form["NewQuantityForItemId" + item.ID] != null))
                        {
                            item.NewOnHand = Int32.Parse(form["NewQuantityForItemId" + item.ID].ToString());
                            item.CounterInitials = this.counterInitials;
                        }

                // Updated the bin location
                        
                        if (form["NewBinLocationForItem" + item.ID] != item.BinLocation)
                        {
                            if(form["NewBinLocationForItem" + item.ID] == "")
                            {
                                item.NewBinLocation = item.BinLocation; // Set to default if it is blank
                            }
                            else
                            {
                                item.NewBinLocation = form["NewBinLocationForItem" + item.ID].ToString();
                                item.CounterInitials = this.counterInitials;
                            }
                        }

                // Updated the Notes
                        if (form["NotesForItemId" + item.ID] != null && form["NotesForItemId" + item.ID] != "")
                        {
                            item.Notes = form["NotesForItemId" + item.ID];
                        }
                        
                    }
                    db.SaveChanges();
                
                return null;
        }

        public List<int> SummaryOfCurrentCount()
        {
            List<int> Summary = new List<int>();
            var items = from m in db.Items
                        where m.OnHand!=0 // For now, we don't want to include Items that have 0 on hand
                        select m;

            int totalItems = items.ToList().Count();
            Summary.Add(totalItems); // First index are the total items in the db

            int totalItemsCounted = items.ToList().Where(x => (x.NewOnHand != x.OnHand) || (x.Confirmed)).Count();
            Summary.Add(totalItemsCounted); // Second index is total number of items counted

            int totalItemsNotCounted = totalItems - totalItemsCounted;
            Summary.Add(totalItemsNotCounted);

            return Summary;
        }

        public ActionResult DeleteDatabase()
        {
            var items = from m in db.Items
                        select m;
            foreach (var item in items)
            {
                db.Items.Remove(item);
            }
            db.SaveChanges();
            return null;
        }

        public ActionResult ColumnCount()
        {
            if (AdminEmails.Contains(this.email))
            {
                Session["NonWeeklyCount"] = true;
                return View();
            }
            else
            {
                return new HttpStatusCodeResult(403);
            }
             
        }
        
        [HttpPost]
        public ActionResult ColumnCount(string binLocation)
        {
            if (AdminEmails.Contains(this.email))
            {
                PopulateColumnDB client = new PopulateColumnDB();
                client.GenerateDB(binLocation.ToUpper(), this.email, this.password);
                return RedirectToAction("Index");
            }
            else
            {
                return new HttpStatusCodeResult(403);
                
            }
        }

        // Check if there are no items left to count to update status in view.
        public bool NoItemsLeftToCount()
        {
            var items = from m in db.Items
                        select m;

            var itemsNotCounted = items.ToList().Where(x => !(x.ChangesMade) && !(x.Confirmed) && (x.OnHand != 0)); // For now, items that have on hand qty do not need to be counted

            
            if (itemsNotCounted.Count() == 0)
            {
                return true; 
            }
            return false; 
        }

        [HttpPost]
        public ActionResult UpdateDatabaseAjax(int ID, bool confirmed, int quantity, string location, string notes)
        {
            var item = db.Items.Single(x => x.ID == ID);

            if(notes=="")
            {
                notes = null;
            }
            item.Confirmed = confirmed;
            item.NewOnHand = quantity;
            if(location != "")
            {
                item.NewBinLocation = location;
            }
            else
            {
                item.NewBinLocation = null; // This is the case if the original location was NULL (for EndCount.cshtml)
            }

            item.Notes = notes;
            item.CounterInitials = this.counterInitials;
            
            db.SaveChanges();

            return PartialView("_TableRow", item);
        }

        [HttpPost]
        public ActionResult UpdateOptionAjax(string binLocation)
        {
                // Grab all items in the bin location and check if they meet the criteria
                IEnumerable<Item> Items = db.Items.Where(x=>x.BinLocation == binLocation).ToList();

                BinLocation bin = new BinLocation();

                bin.Bin = binLocation;

                // Put logic here
                bin.ContainsOnlyNonZeros = Items.All(x => x.OnHand != 0);
                bin.ContainsOnlyCountedItems = Items.All(x => x.ChangesMade || x.Confirmed);
                bin.ContainsUncountedOnHand = Items.Any(x=> (!((x.ChangesMade) || (x.Confirmed)) && (x.OnHand != 0)));
                return PartialView("_OptionView", bin);

        }
    }
}            