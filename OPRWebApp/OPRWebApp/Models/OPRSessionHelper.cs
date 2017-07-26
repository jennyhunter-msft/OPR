using System;
using System.Collections.Generic;
using System.Linq;
using BingMapsRESTToolkit;
using System.Net;
using System.IO;
using System.Web.Configuration;

namespace OPRWebApp.Models
{
    public class OPRSessionHelper
    {
        private static string _cognitiveKey = WebConfigurationManager.AppSettings["CognitiveKey"];
        private static string _cognitiveURI = "https://api.labs.cognitive.microsoft.com/Routes/Matrix?optimize=distance&subscription-key={0}&mode={1}&origins={2}";

        public static bool SessionExists(string sessionId)
        {
            using (var db = new OPRDBEntities())
            {
                var sessionExists = db.Sessions.Any(s => s.SessionID.ToString().Equals(sessionId, StringComparison.InvariantCultureIgnoreCase));
                return sessionExists;
            }
        }

        public static string AddSession()
        {
            var sessionId = Guid.NewGuid();
            using (var db = new OPRDBEntities())
            {
                //add new stop
                Session newSession = new Session();
                newSession.SessionID = sessionId;
                db.Sessions.Add(newSession);
                db.SaveChanges();
            }
            return sessionId.ToString();
        }
        public static string AddPath(string sessionId)
        {
            var pathId = Guid.NewGuid();
            using (var db = new OPRDBEntities())
            {
                //add new stop
                Path newPath = new Path();
                newPath.SessionID = Guid.Parse(sessionId);
                newPath.PathID = pathId;
                db.Paths.Add(newPath);
                db.SaveChanges();
            }
            return pathId.ToString();
        }

        public static bool PathExistsForSession(string sessionId, string pathId)
        {
            using (var db = new OPRDBEntities())
            {
                var pathExists = db.Paths.Any(p => 
                p.SessionID.ToString().Equals(sessionId, StringComparison.InvariantCultureIgnoreCase) && 
                p.PathID.ToString().Equals(pathId,StringComparison.InvariantCultureIgnoreCase));
                return pathExists;
            }
        }

        public static List<string> RetrievePath(string sessionId, string pathId)
        {
            List<string> path = new List<string>();
            using (var db = new OPRDBEntities())
            {
                var stops = db.Stops.Where(st => string.Equals(st.PathID.ToString(), pathId)).OrderBy(st => st.StopOrder).ToList();
                path.AddRange(stops.Select(st => $"{st.StopOrder} : {st.Addr} : {st.Latitude:F12},{st.Longitude:F12}"));
            }
            return path;
        }
        
        public static string AddStopToPath(string sessionId, string pathId, string location)
        {
            Location stopInformation = BingMapsHelper.GetLocationInformation(location);
            Point stopCoordinates = new Point(stopInformation.Point.Coordinates);
            var stopId = Guid.NewGuid();

            using (var db = new OPRDBEntities())
            {
                var stopOrder = db.Stops.Count(st => 
                string.Equals(st.PathID.ToString(), pathId));

                //add new stop
                Stop newStop = new Stop();
                newStop.StopOrder = stopOrder;
                newStop.StopID = stopId;
                newStop.PathID = Guid.Parse(pathId);
                newStop.Addr = stopInformation.Address.FormattedAddress;
                newStop.Latitude = stopCoordinates.Latitude;
                newStop.Longitude = stopCoordinates.Longitude;
                db.Stops.Add(newStop);
                db.SaveChanges();
            }
            return stopId.ToString();
        }

        public static List<string> OptimizePath(string sessionId, string pathId)
        {
            // Number of stop
            int stopLength = 0;
            // List of locations in the format {lat,long;lat,long;lat,long...}
            string locList;
            using (var db = new OPRDBEntities())
            {
                var stops = db.Stops.Where(st => string.Equals(st.PathID.ToString(), pathId)).OrderBy(st => st.StopOrder).ToList();
                stopLength = stops.Count();
                locList = string.Join(";", stops.Select(st => $"{st.Latitude:F12},{st.Longitude:F12}"));
            }

            string uri = string.Format(_cognitiveURI, _cognitiveKey, BingMapsHelper.TransportationMode, locList);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("Ocp-Apim-Subscription-Key:"+_cognitiveKey);

            // Get HTTP response from Abu Dhabi
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // Deserialize the response JSON
            string responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
            System.Web.Script.Serialization.JavaScriptSerializer deSerializedResponse = new System.Web.Script.Serialization.JavaScriptSerializer();
            CognitiveApiReply reply = (CognitiveApiReply)deSerializedResponse.Deserialize(responseString, typeof(CognitiveApiReply));

            // Create a path object to store the new path
            var apiPathResult = new List<CognitiveApiResult>();
            var apiDestinationIndicies = new List<int?>();
            int? currentOrigin = 0;

            // Optimize shit via nearest neighbor
            while (apiPathResult.Count()<stopLength-1)
            {
                
                // Create a sublist of results only with the current origin index
                List<CognitiveApiResult> currentResult = reply.results.Where((item, index) => reply.results[index].originIndex == currentOrigin).ToList();
               
                CognitiveApiResult nearestNeighbor = new CognitiveApiResult();
                nearestNeighbor.travelDistance = Double.MaxValue;


                // Loop through the destinations for the minimum distance
                for (int j=1; j<currentResult.Count(); j++)
                {
                    var currentNeighbor = currentResult[j];
                    if(currentNeighbor.travelDistance < nearestNeighbor.travelDistance && !apiDestinationIndicies.Contains(currentNeighbor.destinationIndex))
                    {
                        nearestNeighbor = currentNeighbor;
                    }
                }

                // Add the next path item to the list
                apiPathResult.Add(nearestNeighbor);
                apiDestinationIndicies.Add(currentOrigin);
                currentOrigin = nearestNeighbor.destinationIndex;
            }

            // Close the response
            response.Close();

            // INSERT some conversation here to convert from the API results to the necessary format for the Bing Maps API call


            List<string> path = new List<string>();
            return path;
        }
    }


    public class CognitiveApiReply
    {
        public bool? isAccepted { get; set; }
        public bool? isCompleted { get; set; }
        public double? callBackSeconds { get; set; }
        public string callBackUrl { get; set; }
        public string errorMessage { get; set; }
        public int? requestId { get; set; }
        public List<CognitiveApiResult> results { get; set; }
    }

    public class CognitiveApiResult
    {
        public double? originLatitude { get; set; }
        public double? originLongitude { get; set; }
        public int? originIndex { get; set; }
        public double? destinationLatitude { get; set; }
        public double? destinationLongitude { get; set; }
        public int? destinationIndex { get; set; }
        public double? travelDistance { get; set; }
        public double? travelDuration { get; set; }
        public string departureTime { get; set; }
    }

        public class Constants
    {
        //Sessions
        public const string EmptySessionID = "EMPTY_SESSION_ID";
        public const string InvalidSessionID = "INVALID_SESSION_ID";

        //Paths
        public const string EmptyPathID = "EMPTY_PATH_ID";
        public const string InvalidPathID = "INVALID_PATH_ID";
    }
}