using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Services.NewFolder
{
    public class PresenceHub : Hub
    {
        PresenceTracker _tracker;
        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var UserName = Context.GetHttpContext().Request.Query["username"];

            await _tracker.UserConnected(UserName, Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOnline", UserName);
            var currentusers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentusers);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var UserName = Context.GetHttpContext().Request.Query["username"];
            await _tracker.UserDisconnected(UserName, Context.ConnectionId);
            await Clients.Others.SendAsync("UserIsOffline", UserName);
            var currentusers = await _tracker.GetOnlineUsers();
            await Clients.All.SendAsync("GetOnlineUsers", currentusers);
            await base.OnDisconnectedAsync(exception);
        }


    }
}
