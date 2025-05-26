using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Interfaces;
using WebApiQuespond.Models;

namespace WebApiQuespond.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AppUserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IPhotoService _photoService;
        public AppUserController(IUserService UserService, ITokenService tokenService, IPhotoService photoService)
        {
            _userService = UserService;
            _tokenService = tokenService;
            _photoService = photoService;
        }

        [HttpGet("AllUsers")]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetAllUsers()
        {
            var users =  await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("member/{username}")]
        public async Task<ActionResult<AppUser>> GetUserByName(string username)
        {
            var user = await _userService.GetUserByUserName(username);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut]
        public async Task<ActionResult<AppUser>> UpdatedUser(AppUser Updateddata)
        {
            var user = await _userService.UpdatedUser(Updateddata);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<Photo>> AddPhoto(IFormFile file)
        {
            String UserName = _tokenService.GetUsernameFromToken();
            if (string.IsNullOrEmpty(UserName)) return BadRequest("User Not Found");
            var result = await _photoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            int newId = await _userService.AddUserFileAsync(UserName, result.OriginalFilename,result.SecureUrl.AbsoluteUri, false);

            if (newId == 0)
            {
                return BadRequest("File not save..!");
            }
            var photo = new Photo
            {
                PhotoId = newId,
                FilePath = result.SecureUrl.AbsoluteUri,
                FileName = result.OriginalFilename,
                IsMain = "No"
            };
           

            return CreatedAtAction(
                nameof(GetUserByName),
                new { username = UserName },
                new { Photo = photo }
            );

        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files)
        {

            String UserName = _tokenService.GetUsernameFromToken();
            if (string.IsNullOrEmpty(UserName)) return BadRequest("User Not Found");

            List<PhotoUpload> Photos = new List<PhotoUpload>();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var result = await _photoService.AddPhotoAsync(file);
                    if (result.Error != null)
                    {
                        Photos.Add(new PhotoUpload
                        {
                            PhotoId = 0,
                            FileName = result.OriginalFilename,
                            FilePath = "",
                            IsMain = "",
                            Status = result.Error.Message,
                            UserName = UserName 
                        });
                    }
                    int newId = await _userService.AddUserFileAsync(UserName, result.OriginalFilename, result.SecureUrl.AbsoluteUri, false);
                    if (newId == 0)
                    {
                        Photos.Add(new PhotoUpload
                        {
                            PhotoId = 0,
                            FileName = result.OriginalFilename,
                            FilePath = "",
                            IsMain = "",
                            Status = "File Path not Save to DB.!",
                            UserName = UserName
                        });
                    }

                    Photos.Add(new PhotoUpload
                    {
                        PhotoId = newId,
                        FileName = result.OriginalFilename,
                        FilePath = result.SecureUrl.AbsoluteUri,
                        IsMain = "No",
                        Status = "File Uploaded Successfully.!",
                        UserName = UserName
                    });
                }
            }
            return Ok(Photos);
        }


        [HttpPut("set-main-photo")]
        public async Task<ActionResult<Photo>> UpdateUserMainPhoto(Photo data)
        {
            var user = await _userService.UpdateUserMainPhoto(data);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpDelete("delete-photo/{id}")]
        public async Task<IActionResult> DeleteUserPhoto(int id)
        {
            bool deleted = await _userService.DeleteUserPhoto(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Photo deleted successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetPaged(int pageNumber = 1, int pageSize = 10,
            string userName = "",string gender = "",string city = "")
        {
            var result = await _userService.GetPagedUsersAsync(pageNumber, pageSize,userName,gender,city);
            return Ok(result);
        }

    }
}
