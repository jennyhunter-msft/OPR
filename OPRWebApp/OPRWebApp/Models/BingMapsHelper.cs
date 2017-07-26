﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Web;
using System.Web.Configuration;
using BingMapsRESTToolkit;

namespace OPRWebApp.Models
{
    public class BingMapsHelper
    {
        private static string _bingMapsKey = WebConfigurationManager.AppSettings["BingMapsKey"];

        private const string _bingMapsBaseUrl = "http://dev.virtualearth.net/REST/v1/";

        private const string _bingMapsLocationURL = _bingMapsBaseUrl + "/Locations?q={0}&key={1}";
        private const string _bingMapsRouteURL = _bingMapsBaseUrl + "/Routes/{0}?{1}$key={2}";
        private const string _baseTransportMode = "Driving";

        public static string GetLocationName(string location)
        {
            return GetLocationInformation(location).Name;
        }

        public static string GetLocationCoordinates(string location)
        {
            var coordinates = new Point(GetLocationInformation(location).Point.Coordinates);
            return coordinates.ToString();
        }

        public static Location GetLocationInformation(string location)
        {
            Uri geoCodeRequest = BingMapsHelper.GenerateLocationURI(location);
            var response = BingMapsHelper.QueryBingMaps(geoCodeRequest);
            return (Location)response.ResourceSets[0].Resources[0];
        }

        public static Uri GenerateLocationURI(string location)
        {
            return new Uri(string.Format(_bingMapsLocationURL, location, _bingMapsKey));
        }

        public static Uri GenerateRouteURI(string transportMode = _baseTransportMode, Leg[] legs = null)
        {
            string legsUriElements = string.Join("&", legs.SelectMany(l => l.GenerateUriElement()));
            return new Uri(string.Format(_bingMapsRouteURL, transportMode, legsUriElements, _bingMapsKey));
        }

        public static Response QueryBingMaps(Uri geoCodeRequest)
        {
            var request = (HttpWebRequest)WebRequest.Create(geoCodeRequest);
            try
            {
                using (var response = (HttpWebResponse) request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        var message = string.Format("Request failed. Received HTTP {0}", response.StatusCode);
                        throw new ApplicationException(message);
                    }

                    // grab the response
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Response));
                    return ser.ReadObject(response.GetResponseStream()) as Response;
                }
            }
            catch (Exception)
            {
                return new Response();
            }
        }
    }

    public class Leg
    {
        public Point Waypoint;
        public Point[] ViaWaypoints;

        public Leg(Point point)
        {
            Waypoint = point;
        }

        public string GenerateUriElement()
        {
            string UriElements = "wayPoint=" + Waypoint;
            foreach (var viaWaypoint in ViaWaypoints)
            {
                UriElements += "&viaWaypoint=" + viaWaypoint;
            }
            return UriElements;
        }
    }

    public class Point
    {
        public double Longitude;
        public double Latitude;

        public Point(double[] d)
        {
            Longitude = d[0];
            Latitude = d[1];
        }

        public Point(double longitude, double latitude)
        {
            Longitude = longitude;
            Latitude = latitude;
        }

        public override string ToString()
        {
            return $"{Longitude},{Latitude}";
        }
    }
}