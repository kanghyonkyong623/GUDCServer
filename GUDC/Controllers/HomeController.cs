using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace GUDC.Controllers
{
    public class HomeController : Controller
    {
        public static SelectList DistrictCategory;
        public ActionResult Index()
        {
            if (WebSecurity.IsAuthenticated)
            {
                ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

                return View();
            } 
            else
            {
                return RedirectToAction("Login","Account");
            }
        }
        

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
