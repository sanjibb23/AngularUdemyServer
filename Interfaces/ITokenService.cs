using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Models;

namespace WebApiQuespond.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
        string GetUsernameFromToken();

    }
}
