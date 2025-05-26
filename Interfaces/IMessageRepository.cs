using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Models;

namespace WebApiQuespond.Interfaces
{
  public  interface IMessageRepository
    {
        Task<int> SaveMessageAsync(int senderId, int receiverId, string content);
        Task MarkAsReadAsync(int messageId);
        Task<List<Message>> GetMessagesAsync(int userId1, int userId2);
    }
}
