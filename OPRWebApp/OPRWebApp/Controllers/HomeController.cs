using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using BingMapsRESTToolkit;
using Microsoft.Ajax.Utilities;
using RestSharp;

namespace OPRWebApp.Controllers
{
    public class HomeController : Controller
    {
        private string _bingMapsKey = WebConfigurationManager.AppSettings["BingMapsKey"];
        private string _bingMapsRESTUrl = "http://dev.virtualearth.net/REST/v1/Locations?q={0}&key={1}";

        public ActionResult Index(string sessionId, string location)
        {
            ViewData["session"] = sessionId;

            if (!string.IsNullOrEmpty(location))
            {
                ViewData["LocationName"] = GetLocationName(location);
            }

            return View();
        }

        public string GetLocationName(string location)
        {
            string result = string.Format("No results found for location : {0}", location);
            result = GetLocationInformations(location).Name;
            return result;
        }

        public string GetLocationCoordinates(string location)
        {
            string result = string.Format("No results found for location : {0}", location);
            var coordinates = GetLocationInformations(location).Point.Coordinates;
            result = string.Format("{0}, {1}", coordinates[0], coordinates[1]);
            return result;
        }

        private Location GetLocationInformations(string location)
        {
            Uri geoCodeRequest = new Uri(string.Format(_bingMapsRESTUrl, location, _bingMapsKey));
            return (Location)QueryBingMaps(geoCodeRequest).ResourceSets[0].Resources[0];
        }

        private Response QueryBingMaps(Uri geoCodeRequest)
        {
            var request = (HttpWebRequest) WebRequest.Create(geoCodeRequest);

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                // grab the response
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));
                return ser.ReadObject(response.GetResponseStream()) as Response;
            }
        }
    }
}