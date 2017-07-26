using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public ActionResult Index(string sessionId=null, string pathId=null)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                ViewBag.SessionId = Constants.EmptySessionID;
            }
            else
            {
                //Confirm that sessionId is valid
                var sessionExists = OPRSessionHelper.SessionExists(sessionId);
                ViewBag.SessionId = sessionExists ? sessionId : Constants.InvalidSessionID;
            }

            if (string.IsNullOrEmpty(pathId))
            {
                ViewBag.PathId = Constants.EmptyPathID;
            }
            else
            {
                //Confirm that sessionId is valid
                var pathExists = OPRSessionHelper.PathExistsForSession(sessionId, pathId);
                ViewBag.PathId = pathExists ? pathId : Constants.InvalidPathID;
                ViewBag.CurrentPath = pathExists ? string.Join("&", OPRSessionHelper.RetrievePath(sessionId, pathId)) : "";
            }

            //TODO; find a better way to share the bingmapskey
            ViewBag.BingMapsKey = BingMapsHelper.BingMapsKey;

            return View();
        }

        private bool IsSessionValid(string sessionId)
        {
            return !(string.IsNullOrEmpty(sessionId) || string.Equals(sessionId, Constants.InvalidSessionID) ||
                     string.Equals(sessionId, Constants.EmptySessionID));
        }
        private bool IsPathValid(string pathId)
        {
            return !(string.IsNullOrEmpty(pathId) || string.Equals(pathId, Constants.InvalidPathID) ||
                     string.Equals(pathId, Constants.EmptyPathID));
        }

        [HttpPost]
        public ActionResult AddSession()
        {
            var session = OPRSessionHelper.AddSession();
            return Json(session);
        }

        [HttpPost]
        public ActionResult AddPath(string sessionId)
        {
            var path = OPRSessionHelper.AddPath(sessionId);
            return Json(path);
        }

        [HttpPost]
        public ActionResult AddLocationToPath(string sessionId, string pathId, string location)
        {
            if (!IsSessionValid(sessionId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { error = "ERROR : Invalid Session ID" });
            }
            if (!IsPathValid(pathId))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json(new { error = "ERROR : Invalid Path ID" });
            }

            OPRSessionHelper.AddStopToPath(sessionId, pathId, location);
            var currentPath = OPRSessionHelper.RetrievePath(sessionId, pathId);

            return Json(currentPath); ;
        }

        [HttpPost]
        public ActionResult OptimizePath(string sessionId, string pathId)
        {
            OPRSessionHelper.OptimizePath(sessionId, pathId);
            var currentPath = OPRSessionHelper.RetrievePath(sessionId, pathId);
            return Json(currentPath); ;
        }

    }
}