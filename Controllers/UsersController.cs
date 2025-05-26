using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApiQuespond.Models;
using WebApiQuespond.Services;

namespace WebApiQuespond.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<List<User>>> GetUsers()
        {
            return await _userService.GetAllUsersAsync();
        }

        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            await _userService.AddUserAsync(user);
            return Ok("User added successfully!");
        }

        [HttpPost("add-multiple-user")]
        public async Task<IActionResult> InsertUsers([FromBody] List<User> users)
        {
            if (users == null || users.Count == 0) return BadRequest("Invalid user data.");
            int SuccNo = 0;

            foreach(User val in users)
            {
                try
                {
                    await _userService.AddUserAsync(val);
                    SuccNo++;
                }
                catch
                {

                }
                
            }
            if(SuccNo > 0) return Ok(SuccNo + " Users added Successfully");
            return StatusCode(500, "Failed to insert users.");            
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.Id)
                return BadRequest("User ID mismatch.");

            var updated = await _userService.UpdateUserAsync(user);
            if (!updated)
                return NotFound("User not found.");

            return Ok("User updated successfully.");
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<AppUser>> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            bool deleted = await _userService.DeleteUserAsync(id);
            if (!deleted) return NotFound();
            return Ok("User deleted successfully.");
        }


        [Authorize]
        [HttpGet("Get-Authorize-user")]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetUsersNew()
        {
            return await _userService.GetAllUsersAsyncNew();
        }

    }
}
