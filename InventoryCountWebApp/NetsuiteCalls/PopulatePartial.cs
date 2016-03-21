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
    public class PopulatePartial
    {

        public List<int> GenerateList(string Email, string Password)
        {
            List<int> itemsOnPartial = new List<int>();

            // Query NetSuite 
            HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create("https://rest.netsuite.com/app/site/hosting/restlet.nl?script=77&deploy=1&savedsearchid=4209");
            objRequest.Method = "GET";
            objRequest.ContentType = "application/json";
            objRequest.Headers.Add(HttpRequestHeader.Authorization, "NLAuth nlauth_account=" + ConfigurationManager.AppSettings["NetSuiteAccountNumber"].ToString() + ", nlauth_email=" + Email + ", nlauth_signature=" + Password + "");
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
                    if(cols!= null)
                    {
                        int ID = cols["internalid"]["internalid"] == null ? 0 : cols["internalid"]["internalid"].Value<int>();
                        itemsOnPartial.Add(ID);
                    }


                }
            }
            return itemsOnPartial;
        }
    }
}