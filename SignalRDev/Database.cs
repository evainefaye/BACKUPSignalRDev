using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNet.SignalR;

namespace SignalR
{
    public class Database
    {
        public static void lookupMonitorUsers(string userId, string connectionId)
        {
            using (SignalR db = new SignalR())
            {
                lookupMonitorUser monitorUser = new lookupMonitorUser();
                var monitorUserRecord = db.lookupMonitorUsers.Where(c => c.userId == userId).SingleOrDefault();
                dynamic results = new System.Dynamic.ExpandoObject();
                if (monitorUserRecord != null)
                {
                    if (monitorUserRecord.connectionId != "" && monitorUserRecord.connectionId != connectionId)
                    {
                        results.resultType = "error";
                        results.resultMessage = "User Already Connected on connectionId: " + monitorUserRecord.connectionId;
                    }
                    else
                    {
                        results.resultType = "success";
                        results.resultMessage = "";
                        results.maximumChats = monitorUserRecord.maximumChats;
                        results.connectionId = connectionId;
                        int maximumChats = monitorUserRecord.maximumChats;
                        monitorUserRecord.connectionId = connectionId;
                        db.Entry(monitorUserRecord).CurrentValues.SetValues(monitorUserRecord);
                        db.SaveChanges();
                        db.Configuration.LazyLoadingEnabled = false;
                        var lookupMonitorUsersRecord = db.lookupMonitorUsers.Where(c => c.userId == userId).SingleOrDefault();
                        results.monitorUser = lookupMonitorUsersRecord;
                        var userRecord = db.users.Where(c => c.userId == userId).SingleOrDefault();
                        results.user = userRecord;
                    }
                }
                else
                {
                    results.resultType = "error";
                    results.resultMessage = "User not found";
                }

                var myHub = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                var json = JsonConvert.SerializeObject(results);
                myHub.Clients.Client(connectionId).setupMonitor(json);
            }
            return;
        }


        /* Takes the input of userId and searches in the database table [users] This function will retrieve the userName from the database table [users] */
        public static string GetUserName(string userId, string agentLocationCode)
        {
            using (SignalR db = new SignalR())
            {
                user user = new user();
                var userRecord = db.users.Where(u => u.userId == userId.ToLower()).SingleOrDefault();
                if (userRecord != null)
                {
                    if (agentLocationCode != "" && (userRecord.locationCode == "" || userRecord.locationCode == null))
                    {
                        userRecord.locationCode = agentLocationCode.ToUpper();
                        db.Entry(userRecord).CurrentValues.SetValues(userRecord);
                        db.SaveChanges();
                        locationLookup locationLookup = new locationLookup();
                        var locationLookupRecord = db.locationLookups.Where(l => l.locationCode == agentLocationCode.ToUpper()).SingleOrDefault();
                        if (locationLookupRecord == null)
                        {
                            locationLookup.locationCode = agentLocationCode.ToUpper();
                            string locationName = "Building: " + agentLocationCode;
                            locationLookup.locationName = locationName;
                            db.locationLookups.Add(locationLookup);
                            db.SaveChanges();
                            var context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                            context.Clients.Group(groupNames.Monitor).addLocationName(agentLocationCode, locationName);
                        }
                    }
                    return userRecord.userName.ToUpper();
                }
                else
                {
                    return null;
                }
            }
        }

        /* Adds a user record to the users table if one does not exist already */
        public static void AddUserRecord(string userId, string userName, string AgentLocationCode)
        {
            using (SignalR db = new SignalR())
            {
                user user = new user();
                user.userId = userId;
                user.userName = userName;
                db.users.Add(user);
                db.SaveChanges();
            }
        }

        /* Adds a record to the SashaSessions Database on connection of a SASHA client */
        public static bool AddSashaSessionRecord(string connectionId, string userId, string smpSessionId, string sessionStartTime, string milestone)
        {
            using (SignalR db = new SignalR())
            {
                sashaSession sashaSession = new sashaSession();
                if (!db.sashaSessions.Any(s => s.connectionId == connectionId))
                {
                    sashaSession.connectionId = connectionId;
                    sashaSession.userId = userId;
                    sashaSession.smpSessionId = smpSessionId;
                    sashaSession.sessionStartTime = sessionStartTime;
                    sashaSession.milestone = milestone;
                    db.sashaSessions.Add(sashaSession);
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        /* Updates the sashaSessionrecord with the current time so that it starts getting tracked on monitors */
        public static void UpdateSashaSessionRecord(string userId, string connectionId)
        {
            using (SignalR db = new SignalR())
            {
                var sashaSessionRecord =
                    (from s in db.sashaSessions
                     where s.userId == userId
                     && s.connectionId == connectionId
                     select s
                    ).FirstOrDefault();
                if (sashaSessionRecord != null)
                {
                    string userName = sashaSessionRecord.user.userName;
                    string sessionStartTime = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH\\:mm\\:ssZ");
                    sashaSessionRecord.sessionStartTime = sessionStartTime;
                    db.Entry(sashaSessionRecord).CurrentValues.SetValues(sashaSessionRecord);
                    db.SaveChanges();
                    var context = GlobalHost.ConnectionManager.GetHubContext<MyHub>();
                    context.Clients.Group(groupNames.Monitor).addSashaSession(connectionId, userId, userName, sessionStartTime);
                }
            }
        }


    }
}