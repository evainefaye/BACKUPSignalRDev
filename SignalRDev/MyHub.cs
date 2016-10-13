using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System.Web;

namespace SignalR
{

    public static class groupNames
    {
        public const string Monitor = "Monitor";
        public const string Sasha = "Sasha";
    }

    public class MyHub : Hub
    {

        // Check if User is Authenticated
        // If not requests login info then continues
        public void CheckAuthenticated()
        {
            if (Context.User.Identity.IsAuthenticated)
            {
                string userId = Context.User.Identity.Name.GetUserId();
                string connectionId = Context.ConnectionId;
                Clients.Caller.userId = userId;
                Clients.Caller.connectionId = connectionId;
                Database.lookupMonitorUsers(userId, connectionId);
            }
            else
            {
                Clients.Caller.getUserId();
            }
        }

        public void AuthenticatedUser(string userId)
        {
            string connectionId = Context.ConnectionId;
            Clients.Caller.userId = userId;
            Clients.Caller.connectionId = connectionId;
            Database.lookupMonitorUsers(userId, connectionId);
        }

        public void RequestDictionary(string monitorId, string sashaId)
        {
            Clients.Client(sashaId).requestDictionary(monitorId);
        }

        public void ReceiveDictionary(string monitorId, string dictionary)
        {
            Clients.Client(monitorId).receiveDictionary(dictionary);
        }


        public void RegisterSashaSession(string userId, string agentName, string smpSessionId, string agentLocationCode)
        {
            string userid = userId.ToLower();
            string connectionId = Context.ConnectionId;
            string userName = Database.GetUserName(userId, agentLocationCode);
            if (userName == null)
            {
                Database.AddUserRecord(userId.ToLower(), agentName.ToUpper(), agentLocationCode.ToUpper());
            }
            if (agentLocationCode != "" && agentLocationCode != null)
            {
                Groups.Add(connectionId, agentLocationCode);
            }
            Clients.Caller.userId = userId.ToLower();
            Clients.Caller.userName = agentName.ToUpper();
            Clients.Caller.smpSessionId = smpSessionId;
            Groups.Add(connectionId, smpSessionId);
            string sessionStartTime = "";
            if (Database.AddSashaSessionRecord(connectionId, userId, smpSessionId, sessionStartTime, ""))
            {
                Groups.Add(connectionId, groupNames.Sasha);
                Groups.Add(connectionId, smpSessionId);
                Clients.Group(groupNames.Monitor).addSashaSession(connectionId, userId, userName, sessionStartTime, "");
            }
        }

        /* Sets the Session Start Time to a value indicating that you have begun the actual SASHA flow and should be tracked */
        public void UpdateSashaSession()
        {
            string userId = Clients.Caller.userId;
            string connectionId = Context.ConnectionId;
            Database.UpdateSashaSessionRecord(userId, connectionId);
        }


    }
}