using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebApiQuespond.Interfaces;
using WebApiQuespond.Models;
using WebApiQuespond.Services;

namespace WebApiQuespond.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserService _userService;

        private readonly ITokenService _tokenService;

        public AccountController(UserService userService, ITokenService tokenService)
        {
            _userService  = userService;
            _tokenService = tokenService;
        }


        [HttpPost("register")]
        public async Task<ActionResult<UserTokenDTO>> Register(LoginDTO logindata)
        {

            try
            {
                string username = logindata.UserName;
                string Password = logindata.Password;

                if (removeSpecialChar(username) != "" && removeSpecialChar(Password) != "")
                {
                    if (await UserExist(username.ToLower())) return BadRequest("User Already Taken");

                    var user = new AppUser();
                    using (var hmac = new HMACSHA512())
                    {
                        user.UserName = username;
                        user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(Password));
                        user.PasswordSalt = hmac.Key;
                    };
                    
                    await _userService.RegisterUserAsync(user);

                    return new UserTokenDTO
                    {
                        UserName = user.UserName,
                        Token = _tokenService.CreateToken(user)
                    };

                  //  return user;
                }
                else
                {
                    return BadRequest();
                }
                
            }
            catch(Exception ex)
            {
                return Conflict(ex.Message);
            }
           
        }

        private string removeSpecialChar(string input)
        {
            string result = "";
            try
            {
              result =   Regex.Replace(input, "[^a-zA-Z0-9 ]", "");
            }
            catch
            {
                result = "";
            }

            return result;
        }
        public async Task<bool> UserExist(string UserName)
        {
            var user = await _userService.CheckIfUserExist(UserName);
            if (user == null) return false;
            return true;
        }



        public async Task<AppUser> getDataByUserID(string UserName)
        {
            var user = await _userService.CheckIfUserExist(UserName);
            return user;
        }



        [HttpPost("login")]
        public async Task<ActionResult<UserTokenDTO>> login(LoginDTO logindata)
        {

            try
            {
                if (ModelState.IsValid)
                {

                    AppUser data = await getDataByUserID(logindata.UserName);
                    if(data == null)
                    {
                        return Unauthorized("Invalid UserName");
                    }

                    bool ismatching = false;
                    using (var hmac = new HMACSHA512(data.PasswordSalt))
                    {
                        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(logindata.Password));
                        ismatching =  computedHash.SequenceEqual(data.PasswordHash);
                    }

                    if (!ismatching)
                    {
                        return Unauthorized("Invalid Password");
                    }

                    return new UserTokenDTO
                    {
                        UserName = data.UserName,
                        Token = _tokenService.CreateToken(data),
                        Id = data.Id
                    };

                  //  return data;
                }
                else
                {
                    return BadRequest();
                }

            }
            catch (Exception ex)
            {
                return Conflict(ex.Message);
            }

        }

    }
}
