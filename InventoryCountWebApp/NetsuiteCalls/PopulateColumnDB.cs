using InventoryCountWebApp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace InventoryCountWebApp.Database
{
    public class PopulateColumnDB
    {
        private ItemDBContext db = new ItemDBContext();

        public void GenerateDB(string location, string Email, string Password)
        {

            // Generate partial shelf items
            PopulatePartial partialCreation = new PopulatePartial();
            List<int> partialIds = partialCreation.GenerateList(Email, Password);


            // Query NetSuite 
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("https://rest.netsuite.com/app/site/hosting/restlet.nl?script=131&deploy=1&binlocation=" + location);
            objRequest.Method = "GET";
            objRequest.ContentType = "application/json";
            objRequest.Headers.Add(HttpRequestHeader.Authorization, "NLAuth nlauth_account="+ ConfigurationManager.AppSettings["NetSuiteAccountNumber"].ToString()+", nlauth_email="+Email+", nlauth_signature="+Password+"");
            objRequest.Timeout = 500000;
            HttpWebResponse objResponse = objRequest.GetResponse() as HttpWebResponse;

            // Pull data off of NetSuite
            using (Stream objResponseStream = objResponse.GetResponseStream())
            {
                StreamReader sr = new StreamReader(objResponseStream);
                string rawData = sr.ReadToEnd();
                JArray rows = JArray.Parse(rawData);

                // List that will store all of the Count Items
                List<Item> Items = new List<Item>();

                // Parse Json and store values
                foreach (JObject row in rows)
                {
                    // Create new Count Item
                    Item item = new Item();

                    JToken cols = row["columns"];
                    string Name = cols["itemid"] == null ? null : cols["itemid"].Value<string>();
                    string BinLocation = cols["custitem_bin_location"] == null ? null : cols["custitem_bin_location"].Value<string>();
                    string Description = cols["salesdescription"] == null ? null : cols["salesdescription"].Value<string>();
                    int OnHand = cols["locationquantityonhand"] == null ? 0 : cols["locationquantityonhand"].Value<int>();
                    int ID = cols["internalid"]["internalid"] == null ? 0 : cols["internalid"]["internalid"].Value<int>();
                    string VendorCode = cols["vendorcode"] == null ? null : cols["vendorcode"].Value<string>();
                    string Brand = cols["custitem_brand"] == null ? null : cols["custitem_brand"]["name"].Value<string>();
                    int QuantityBackOrdered = cols["quantitybackordered"] == null ? 0 : cols["quantitybackordered"].Value<int>();
                    string Vendor = cols["othervendor"] == null ? null : cols["othervendor"]["name"].Value<string>();
                    string PreferredVendor = cols["vendor"] == null ? null : cols["vendor"]["name"].Value<string>();
                    int WebID = cols["custitem_web_product_id"] == null ? 0 : cols["custitem_web_product_id"].Value<int>();

                    if (Vendor == PreferredVendor || Vendor == null) // Some items have no vendor, but still need to be counted
                    {
                        // Create the items
                        item.Name = Name;
                        item.BinLocation = BinLocation;
                        item.Description = Description;
                        item.OnHand = OnHand;
                        item.ID = ID;
                        item.NewOnHand = OnHand;
                        item.NewBinLocation = BinLocation;
                        item.CounterInitials = null;
                        item.VendorCode = VendorCode;
                        item.Brand = Brand;
                        item.QuantityBackOrdered = QuantityBackOrdered;
                        item.WebProductID = WebID;
                        if(partialIds.Contains(item.ID))
                        {
                            item.OnPartial = true;
                        }

                        // Add item to the Items list
                        db.Items.Add(item);
                    }
                   
                }
                db.SaveChanges();
            }

        }
    }
}