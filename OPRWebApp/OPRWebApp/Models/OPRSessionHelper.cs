using System;
using System.Collections.Generic;
using System.Linq;
using BingMapsRESTToolkit;

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

        public static List<string> AddStopToPath(string sessionId, string pathId, string location)
        {
            Location stopInformation = BingMapsHelper.GetLocationInformation(location);
            Point stopCoordinates = new Point(stopInformation.Point.Coordinates);
            List<string> currentPath = new List<string>();

            using (var db = new OPRDBEntities())
            {
                var stopOrder = db.Stops.Count(st => 
                string.Equals(st.PathID.ToString(), pathId));

                //add new stop
                Stop newStop = new Stop();
                newStop.StopOrder = stopOrder;
                newStop.StopID = Guid.NewGuid();
                newStop.PathID = Guid.Parse(pathId);
                newStop.Addr = stopInformation.Address.FormattedAddress;
                newStop.Longitude = stopCoordinates.Longitude;
                newStop.Latitude = stopCoordinates.Latitude;
                db.Stops.Add(newStop);
                db.SaveChanges();

                //retrieve current path
                currentPath = db.Stops.Where(st => string.Equals(st.PathID.ToString(), pathId)).Select(st => st.StopOrder + " - " + st.Addr + " : " + st.Longitude + "," + st.Latitude).ToList();
            }

            return currentPath;
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