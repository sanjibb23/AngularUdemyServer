using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.DataAccess;
using WebApiQuespond.Interfaces;
using WebApiQuespond.Models;

namespace WebApiQuespond.Services
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DbHelper _dbHelper;
        private readonly ITokenService _tokenService;

        public MessageRepository(DbHelper dbHelper, ITokenService tokenService)
        {
            _dbHelper = dbHelper;
            _tokenService = tokenService;
        }

        public async Task<int> SaveMessageAsync(int senderId, int receiverId, string content)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("INSERT INTO Messages (SenderId, ReceiverId, Content)" +
                " OUTPUT INSERTED.Id VALUES (@SenderId, @ReceiverId, @Content)", conn);
            cmd.Parameters.AddWithValue("@SenderId", senderId);
            cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
            cmd.Parameters.AddWithValue("@Content", content);
            await conn.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task MarkAsReadAsync(int messageId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("UPDATE Messages SET IsRead = 1 WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", messageId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Message>> GetMessagesAsync(int userId1, int userId2)
        {
            var messages = new List<Message>();
            using var conn = _dbHelper.GetConnection();

            var sql = "";
            if(userId1 == userId2)
            {
                sql = "SELECT * FROM Messages WHERE SenderId = @User1  ORDER BY Timestamp";
            }
            else
            {
                sql = "SELECT * FROM Messages WHERE (SenderId = @User1 AND ReceiverId = @User2) OR (SenderId = @User2 AND ReceiverId = @User1) ORDER BY Timestamp";
            }

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@User1", userId1);
            if(userId1 != userId2) cmd.Parameters.AddWithValue("@User2", userId2);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                messages.Add(new Message
                {
                    Id = reader.GetInt32(0),
                    SenderId = reader.GetInt32(1),
                    ReceiverId = reader.GetInt32(2),
                    Content = reader.GetString(3),
                    Timestamp = reader.GetDateTime(4),
                    IsRead = reader.GetBoolean(5)
                });
            }
            return messages;
        }

    }
}
