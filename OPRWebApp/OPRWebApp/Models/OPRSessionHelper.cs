using System;
using System.Collections.Generic;
using System.Linq;
using BingMapsRESTToolkit;
using System.Net;
using System.IO;

namespace OPRWebApp.Models
{
    public class OPRSessionHelper
    {
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
                path.AddRange(stops.Select(st => $"{st.StopOrder} : {st.Addr} : {st.Longitude:F12},{st.Latitude:F12}"));
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
                newStop.Longitude = stopCoordinates.Longitude;
                newStop.Latitude = stopCoordinates.Latitude;
                db.Stops.Add(newStop);
                db.SaveChanges();
            }
            return stopId.ToString();
        }

        public static List<string> OptimizePath(string sessionId, string pathId)
        {
            // List of locations in the format {lat,long;lat,long;lat,long...}
            string locList;
            List<string> path = new List<string>();
            using (var db = new OPRDBEntities())
            {
                var stops = db.Stops.Where(st => string.Equals(st.PathID.ToString(), pathId)).OrderBy(st => st.StopOrder).ToList();
                
            }
            string cognitiveKey = "INSERT ABU DHABI KEY HERE";
            string uri = "https://api.labs.cognitive.microsoft.com/Routes/Matrix?optimize=distance&subscription-key=" + cognitiveKey + "&mode=walking&origins=" + locList;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Headers.Add("Ocp-Apim-Subscription-Key="+cognitiveKey);

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            // Get the stream associated with the response.
            Stream receiveStream = response.GetResponseStream();

            // Pipes the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(receiveStream, System.Text.Encoding.UTF8);
            Console.WriteLine(readStream.ReadToEnd());
            response.Close();
            readStream.Close();

            return path;
        }
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