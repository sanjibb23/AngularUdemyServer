using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Services.NewFolder
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, List<string>> onlineUsers = new Dictionary<string, List<string>>();

        public Task UserConnected(string username,string connectionid)
        {
            lock (onlineUsers)
            {
                if (onlineUsers.ContainsKey(username))
                {
                    onlineUsers[username].Add(connectionid);
                }
                else
                {
                    onlineUsers.Add(username, new List<string> { connectionid });
                }
            }
            return Task.CompletedTask;
        }
        public Task UserDisconnected(string username, string connectionid)
        {
            lock (onlineUsers)
            {
                if (!onlineUsers.ContainsKey(username))
                {
                    return Task.CompletedTask;
                }
                else
                {
                    onlineUsers[username].Remove(connectionid);
                }

                if(onlineUsers.ContainsKey(username))
                {
                    onlineUsers.Remove(username);
                }
            }
            return Task.CompletedTask;
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] OnlineUsers;
            lock (onlineUsers)
            {
                OnlineUsers = onlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }
            return Task.FromResult(OnlineUsers);
        }

    }
}
