using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiQuespond.Interfaces
{
  public  interface IConnectionRepository
    {
        Task AddConnectionAsync(int userId, string connectionId);
        Task RemoveConnectionAsync(string connectionId);
        Task<List<string>> GetConnectionsByUserId(int userId);
        Task<int> GetUserIdByConnectionId(string connectionId);

    }
}
