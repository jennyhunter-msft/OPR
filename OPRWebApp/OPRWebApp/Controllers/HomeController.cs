using System;
using System.Web.Configuration;
using System.Web.Mvc;
using BingMapsRESTToolkit;
using Microsoft.Ajax.Utilities;

namespace OPRWebApp.Controllers
{
    public class HomeController : Controller
    {
        private string _bingMapsKey = null;

        private void Initialize()
        {
            _bingMapsKey = WebConfigurationManager.AppSettings["BingMapsKey"];
        }

        public ActionResult Index(string sessionId, string query)
        {
            Initialize();
            ViewData["session"] = sessionId;
            ViewData["BingMapsKey"] = _bingMapsKey;

            if (!string.IsNullOrEmpty(query))
            {
                ViewData["LocationInformation"] = GetLocationInformation(query);
            }

            return View();
        }

        public string GetLocationInformation(string query)
        {
            string result = string.Format("No results found for query : {0}", query);
            
            var response = ServiceManager.GetResponseAsync(new GeocodeRequest()
            {
                BingMapsKey = _bingMapsKey,
                Query = query
            }).GetAwaiter().GetResult();

            if (response != null && response.ResourceSets != null &&
                response.ResourceSets.Length > 0 &&
                response.ResourceSets[0].Resources != null &&
                response.ResourceSets[0].Resources.Length > 0)
            {
                for (var i = 0; i < response.ResourceSets[0].Resources.Length; i++)
                {
                    result = (response.ResourceSets[0].Resources[i] as Location).Name;
                }
            }

            return result;
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