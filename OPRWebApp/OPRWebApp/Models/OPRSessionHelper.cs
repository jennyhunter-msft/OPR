using System;
using System.Linq;

namespace OPRWebApp.Models
{
    public class OPRSessionHelper
    {
        public bool SessionExists(string sessionId)
        {
            using (var db = new OPRDBEntities())
            {
                var sessionExists = db.Sessions.Any(s => s.SessionID.ToString().Equals(sessionId,StringComparison.InvariantCultureIgnoreCase));
                return sessionExists;
            }
        }
    }

    public class Constants
    {
        public const string EmptySessionID = "EMPTY_SESSION_ID";
        public const string InvalidSessionID = "INVALID_SESSION_ID";
    }
}