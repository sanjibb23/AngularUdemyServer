using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Interfaces;
using WebApiQuespond.Models;

namespace WebApiQuespond.Services
{
    public class ChatHub : Hub
    {
        private readonly IUserService _userRepo;
        private readonly IMessageRepository _messageRepo;
        private readonly IConnectionRepository _connRepo;

        public ChatHub(IUserService userRepo, IMessageRepository messageRepo, 
            IConnectionRepository connRepo)
        {
            _userRepo = userRepo;
            _messageRepo = messageRepo;
            _connRepo = connRepo;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext().Request.Query["userId"];
            await _userRepo.SetOnlineAsync(int.Parse(userId), true);
            await _connRepo.AddConnectionAsync(int.Parse(userId), Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            var userId = await _connRepo.GetUserIdByConnectionId(Context.ConnectionId);
            await _userRepo.SetOnlineAsync(userId,false);
            await _connRepo.RemoveConnectionAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(ex);
        }

        public async Task SendMessage(int senderId, int receiverId, string message)
        {
            var msgId = await _messageRepo.SaveMessageAsync(senderId, receiverId, message);
            var connIds = await _connRepo.GetConnectionsByUserId(receiverId);
            foreach (var conn in connIds)
            {
                await Clients.Client(conn).SendAsync("ReceiveMessage", senderId, message, msgId);
            }
        }

        public async Task MarkAsRead(int messageId)
        {
            await _messageRepo.MarkAsReadAsync(messageId);
        }

       


    }

}
