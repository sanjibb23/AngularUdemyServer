using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Models;

namespace WebApiQuespond.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<AppUser>> GetAllUsersAsync();
        Task<AppUser> GetUserByUserName(string username);
        Task<AppUser> UpdatedUser(AppUser Updateddata);

        Task<int> AddUserFileAsync(string UserName, string fileName, string filePath, bool isMain);


        Task<Photo> UpdateUserMainPhoto(Photo data);

        Task<bool> DeleteUserPhoto(int id);

        Task<PaginatedResult<AppUser>> GetPagedUsersAsync(int pageNumber, int pageSize, string userName,string gender, string city);

        Task SetOnlineAsync(int userId,bool isOnline);


    }
    
    
}
