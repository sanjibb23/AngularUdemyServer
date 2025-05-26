using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Models;

namespace WebApiQuespond.Interfaces
{
   public interface ILikesService
    {

        Task<bool> ToggleLike(LikeModel model);

        Task<int> GetLikeCount(int itemId);

        Task<IEnumerable<AppUser>> GetAllUsersAsync(string type);

    }
   
}
