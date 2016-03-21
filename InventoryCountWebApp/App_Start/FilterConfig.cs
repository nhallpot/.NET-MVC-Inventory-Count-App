using InventoryCountWebApp.IPFilter;
using System.Web;
using System.Web.Mvc;

namespace InventoryCountWebApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new FilterIPAttribute());
        }
    }
}
