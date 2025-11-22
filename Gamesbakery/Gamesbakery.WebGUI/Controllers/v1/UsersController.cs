using System;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Gamesbakery.WebGUI.Extensions;

namespace Gamesbakery.WebGUI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = User.GetUserId();
                if (userId == null)
                    return Unauthorized();

                var role = User.GetRole();
                var profile = await _userService.GetUserByIdAsync(userId.Value, userId, role);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get user profile" });
            }
        }

        [HttpPatch("balance")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceDTO request)
        {
            try
            {
                var userId = User.GetUserId();
                var role = User.GetRole();

                var profile = await _userService.UpdateBalanceAsync(userId.Value, request.Balance, userId, role);
                return Ok(profile);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class UpdateBalanceDTO
    {
        public decimal Balance { get; set; }
    }
}