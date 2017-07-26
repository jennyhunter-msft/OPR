using System;
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

        private const string _bingMapsLocationUrl = _bingMapsBaseUrl + "/Locations?q={0}&key={1}";
        private const string _bingMapsRouteUrl = _bingMapsBaseUrl + "/Routes/{0}?{1}$key={2}";
        private const string _baseTransportMode = "Walking";

        public static string BingMapsKey => _bingMapsKey;
        public static string TransportationMode => _baseTransportMode;

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
            return new Uri(string.Format(_bingMapsLocationUrl, location, _bingMapsKey));
        }

        public static Uri GenerateRouteURI(string transportMode = _baseTransportMode, Leg[] legs = null)
        {
            string legsUriElements = string.Join("&", legs.SelectMany(l => l.GenerateUriElement()));
            return new Uri(string.Format(_bingMapsRouteUrl, transportMode, legsUriElements, _bingMapsKey));
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
        public double Latitude;
        public double Longitude;

        public Point(double[] d)
        {
            Latitude = d[0];
            Longitude = d[1];
        }

        public Point(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return string.Format("{0:F12},{1:F12}", Latitude, Longitude);
        }
    }
}