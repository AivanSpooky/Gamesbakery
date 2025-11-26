using System;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.Repositories;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.V2
{
    /// <summary>
    /// Controller for managing user profiles.
    /// </summary>
    [ApiController]
    [Route("api/v2/users")]
    [Authorize]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IUserService userService;

        public UsersController(IUserRepository userRepository, IUserService userService)
        {
            this.userRepository = userRepository;
            this.userService = userService;
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<UserListDTO>))]
        public async Task<ActionResult> GetAllUsers(int page = 1, int limit = 1000, bool getAll = false)
        {
            var role = this.User.GetRole();
            var users = await this.userRepository.GetAllAsync(role);
            var totalCount = users.Count();

            if (getAll)
            {
                limit = totalCount; // Fetch all
                page = 1;
            }

            var pagedUsers = users.Skip((page - 1) * limit).Take(limit).Select(u => new UserListDTO
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                IsBlocked = u.IsBlocked,
            }).ToList();

            return this.Ok(new PaginatedResponse<UserListDTO>
            {
                TotalCount = totalCount,
                Items = pagedUsers,
                NextPage = (page * limit < totalCount) ? page + 1 : null,
                PreviousPage = page > 1 ? page - 1 : null,
                CurrentPage = page,
                PageSize = limit,
            });
        }

        [HttpPost("admin/{userId}/ban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BanUser(Guid userId)
        {
            await this.userService.BlockUserAsync(userId, this.User.GetRole());
            return this.NoContent();
        }

        [HttpPost("admin/{userId}/unban")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UnbanUser(Guid userId)
        {
            await this.userService.UnblockUserAsync(userId, this.User.GetRole());
            return this.NoContent();
        }

        /// <summary>
        /// Retrieves a user profile by ID.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user profile details.</returns>
        /// <response code="200">Returns the user profile.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="500">If an error occurs while retrieving the profile.</response>
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<UserResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetUserProfile(Guid userId)
        {
            try
            {
                var role = this.User.GetRole();
                var currentUserId = this.User.GetUserId();
                if (role == UserRole.Guest)
                    return this.Forbid();
                if (role == UserRole.Admin || userId == currentUserId)
                {
                    var userProfile = await this.userService.GetUserByIdAsync(userId, currentUserId, role);
                    return this.Ok(new SingleResponse<UserResponseDTO>
                    {
                        Item = new UserResponseDTO
                        {
                            Id = userProfile.Id,
                            Username = userProfile.Username,
                            Email = userProfile.Email,
                            RegistrationDate = userProfile.RegistrationDate,
                            Country = userProfile.Country,
                            Balance = userProfile.Balance,
                            TotalSpent = userProfile.TotalSpent,
                        },
                        Message = "User profile retrieved successfully",
                    });
                }

                return this.Forbid();
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, new { error = "Failed to retrieve user profile", details = ex.Message });
            }
        }

        /// <summary>
        /// Partially updates a user profile.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="request">The updated user details.</param>
        /// <returns>The updated user profile.</returns>
        /// <response code="200">User profile successfully updated.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the user is not found.</response>
        /// <response code="500">If an error occurs while updating the profile.</response>
        [HttpPatch("{userId}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<UserResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> PartialUpdateUser(Guid userId, [FromBody] UserUpdateDTO request)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(this.ModelState);
            try
            {
                var currentUserId = this.User.GetUserId();
                var role = this.User.GetRole();
                if (userId != currentUserId && role != UserRole.Admin)
                    return this.Forbid("Only admins can update other users");
                var user = await this.userRepository.GetByIdAsync(userId, role);
                if (user == null)
                    return this.NotFound(new { error = "User not found" });
                if (request.Balance.HasValue)
                {
                    if (request.Balance.Value <= 0)
                        return this.BadRequest(new { error = "Balance increment must be positive" });
                    user.Balance += request.Balance.Value;  // Changed to add instead of set
                }

                if (!string.IsNullOrEmpty(request.Country))
                    user.Country = request.Country;
                var updated = await this.userRepository.UpdateAsync(user, role);
                return this.Ok(new SingleResponse<UserResponseDTO>
                {
                    Item = new UserResponseDTO
                    {
                        Id = updated.Id,
                        Username = updated.Username,
                        Email = updated.Email,
                        RegistrationDate = updated.RegistrationDate,
                        Country = updated.Country,
                        Balance = updated.Balance,
                        TotalSpent = updated.TotalSpent,
                        Role = role.ToString(),  // Added role
                    },
                    Message = "User profile updated successfully",
                });
            }
            catch (KeyNotFoundException ex)
            {
                return this.NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return this.StatusCode(500, new { error = "Failed to update user profile", details = ex.Message });
            }
        }
    }
}
