using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.DataAccess;
using WebApiQuespond.Interfaces;

namespace WebApiQuespond.Services
{
    public class ConnectionRepository: IConnectionRepository
    {

        private readonly DbHelper _dbHelper;
        private readonly ITokenService _tokenService;
        public ConnectionRepository(DbHelper dbHelper, ITokenService tokenService)
        {
            _dbHelper = dbHelper;
            _tokenService = tokenService;
        }

        public async Task AddConnectionAsync(int userId, string connectionId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("INSERT INTO Connections (UserId, ConnectionId) VALUES (@UserId, @ConnectionId)", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ConnectionId", connectionId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("DELETE FROM Connections WHERE ConnectionId = @ConnectionId", conn);
            cmd.Parameters.AddWithValue("@ConnectionId", connectionId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
        public async Task<List<string>> GetConnectionsByUserId(int userId)
        {
            var connections = new List<string>();
            using var conn = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT ConnectionId FROM Connections WHERE UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                connections.Add(reader.GetString(0));
            }
            return connections;
        }

        public async Task<int> GetUserIdByConnectionId(string connectionId)
        {
            using var conn = _dbHelper.GetConnection();
            using var cmd = new SqlCommand("SELECT UserId FROM Connections WHERE ConnectionId = @ConnectionId", conn);
            cmd.Parameters.AddWithValue("@ConnectionId", connectionId);
            await conn.OpenAsync();
            return (int)(await cmd.ExecuteScalarAsync() ?? 0);
        }


    }
}
