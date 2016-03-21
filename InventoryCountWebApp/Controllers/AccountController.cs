using Instrumart.SuiteTalk.WebService;
using InventoryCountWebApp.Models;
using System;
using System.Configuration;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace InventoryCountWebApp.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult LogIn()
        {
            LogInModel model = new LogInModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult LogIn(LogInModel model)
        {
            var email = model.Email + "@instrumart.com";

            // Log-in through Netsuite:
            if (ModelState.IsValid)
            {
                NetSuiteService service = new NetSuiteService();

                try
                {
                    var login = service.xLogin(Int32.Parse(ConfigurationManager.AppSettings["NetSuiteAccountNumber"]), email, model.Password, Int32.Parse(ConfigurationManager.AppSettings["NetSuiteRoleNumber"]));
                    Session["NetSuiteService"] = service;
                    var counterID = login.userId.internalId;
                    Employee employee = service.xGetRecord<Employee>(counterID as string);
                    Session["CounterInitials"] = employee.initials as string;
                }
                catch (System.Web.Services.Protocols.SoapException)
                {
                    ModelState.AddModelError("Email", "Invalid Email or Password. Try again.");
                }
               
            }

            if (ModelState.IsValid)
            {
                // Brings user to the page they wanted to go to 
                FormsAuthentication.SetAuthCookie(model.Email, false);
                Session["Email"] = email;
                Session["Password"] = model.Password;
                return RedirectToAction("Index","Home");

            }
            else
            {
                return View(model);
            }

        }

        public ActionResult LogOut()
        {
            return View();
        }
        
        [HttpPost]
        public ActionResult LogOut(string confirm)
        {
            if(confirm.Equals("Log-Out"))
            {
                Session["NetSuiteService"] = null;
                return RedirectToAction("LogIn");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

    }
}