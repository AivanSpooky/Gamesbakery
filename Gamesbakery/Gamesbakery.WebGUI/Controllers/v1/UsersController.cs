using System;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService userService;

        public UsersController(IUserService userService)
        {
            this.userService = userService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = this.User.GetUserId();
                if (userId == null)
                    return this.Unauthorized();

                var role = this.User.GetRole();
                var profile = await this.userService.GetUserByIdAsync(userId.Value, userId, role);
                return this.Ok(profile);
            }
            catch (Exception)
            {
                return this.StatusCode(500, new { error = "Failed to get user profile" });
            }
        }

        [HttpPatch("balance")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> UpdateBalance([FromBody] UpdateBalanceDTO request)
        {
            try
            {
                var userId = this.User.GetUserId();
                var role = this.User.GetRole();

                var profile = await this.userService.UpdateBalanceAsync(userId.Value, request.Balance, userId, role);
                return this.Ok(profile);
            }
            catch (Exception ex)
            {
                return this.BadRequest(new { error = ex.Message });
            }
        }
    }
}
