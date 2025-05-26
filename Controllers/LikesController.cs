using Microsoft.AspNetCore.Authorization;
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
    public class LikesController : ControllerBase
    {
        private readonly ILikesService _likesService;
        public LikesController(ILikesService LikesService)
        {
            _likesService = LikesService;
           
        }


        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleLike([FromBody] LikeModel model)
        {
            bool result = await _likesService.ToggleLike(model);
            //if (!result) return NotFound();
            return Ok(result);
        }

        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetLikeCount(int itemId)
        {
            int result = await _likesService.GetLikeCount(itemId);
            return Ok(new { Likes = result });
        }

        [HttpGet("GetUsers/{type}")]
        public async Task<ActionResult<IEnumerable<AppUser>>> GetAllUsers(string type)
        {
            var users = await _likesService.GetAllUsersAsync(type);
            return Ok(users);
        }
    }
}
