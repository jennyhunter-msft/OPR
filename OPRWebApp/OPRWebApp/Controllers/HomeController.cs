using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Configuration;
using System.Web.Mvc;
using BingMapsRESTToolkit;
using Microsoft.Ajax.Utilities;
using OPRWebApp.Models;
using RestSharp;
using Point = OPRWebApp.Models.Point;

namespace OPRWebApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string sessionId)
        {
            ViewData["session"] = sessionId;
            return View();
        }
        
        public string AddLocationToRouteLeg(string location)
        {
            var locationInformation = GetLocationInformation(location);
            string locationName = locationInformation.Name;
            Point locationCoordinates = new Point(locationInformation.Point.Coordinates);

            //TODO: Query the database to keep track of the current route, add the element to it, and return the full route
            string currentRoute = string.Format("{0} : {1}", locationName, locationCoordinates);
            return currentRoute;
        }

        public string GetLocationName(string location)
        {
            return GetLocationInformation(location).Name;
        }

        public string GetLocationCoordinates(string location)
        {
            var coordinates = new Point(GetLocationInformation(location).Point.Coordinates);
            return coordinates.ToString();
        }

        private Location GetLocationInformation(string location)
        {
            Uri geoCodeRequest = BingMapsHelper.GenerateLocationURI(location);
            return (Location)BingMapsHelper.QueryBingMaps(geoCodeRequest).ResourceSets[0].Resources[0];
        }
    }
}