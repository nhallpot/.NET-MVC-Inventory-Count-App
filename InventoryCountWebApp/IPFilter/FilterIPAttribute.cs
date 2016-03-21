using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace InventoryCountWebApp.IPFilter
{
    public class FilterIPAttribute : FilterAttribute,IActionFilter
    {
        internal class Http403Result : ActionResult
        {
            public override void ExecuteResult(ControllerContext context)
            {
                // Set the response code to 403.
                context.HttpContext.Response.StatusCode = 403;
            }
        }
        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var clientIP = HttpContext.Current.Request.UserHostAddress as string;

            if (clientIP == ConfigurationManager.AppSettings["AllowedIPAddress"] || clientIP == "::1")
            {

            }
            else
            {
                filterContext.Result = new Http403Result();
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
    }
}