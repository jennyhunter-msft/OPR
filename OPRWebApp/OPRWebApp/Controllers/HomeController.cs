using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using BingMapsRESTToolkit;

namespace OPRWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string query)
        {
            ViewData["query"] = query;  
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}