using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApiQuespond.DataAccess;
using WebApiQuespond.Interfaces;
using WebApiQuespond.Models;

namespace WebApiQuespond.Services
{
    public class TokenService : ITokenService
    {

        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TokenService(IConfiguration config, IHttpContextAccessor httpContextAccessor)
        {
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }
        public string CreateToken(AppUser user)
        {
            var tokenKey = _config["TokenKey"] ?? throw new Exception("Can not asscess Token Key");
            if (tokenKey.Length < 64) throw new Exception("Your token key needs to be longer");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));


            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.UserName)
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandeler = new JwtSecurityTokenHandler();
            var token = tokenHandeler.CreateToken(tokenDescriptor);
            return tokenHandeler.WriteToken(token);

        }


        public string GetUsernameFromToken()
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var handler = new JwtSecurityTokenHandler();
                try
                {
                    var jwtToken = handler.ReadJwtToken(token);

                    var usernameClaim = jwtToken.Claims.FirstOrDefault(c =>
                        c.Type == "unique_name" || c.Type == "name" || c.Type == "sub" || c.Type == "email" || c.Type == "nameid");

                    return usernameClaim?.Value ?? "";
                }
                catch
                {
                    return "";
                }
            }
            return "";
        }




    }
}
