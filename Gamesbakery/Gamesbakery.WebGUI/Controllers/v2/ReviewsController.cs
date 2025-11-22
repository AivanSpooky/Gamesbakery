using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.v2
{
    /// <summary>
    /// Controller for managing game reviews.
    /// </summary>
    [ApiController]
    [Route("api/v2/reviews")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        /// <summary>
        /// Creates a new review for a game.
        /// </summary>
        /// <param name="dto">The review creation details.</param>
        /// <returns>The created review.</returns>
        /// <response code="201">Review successfully created.</response>
        /// <response code="400">If the review data is invalid.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpPost]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<ReviewResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> CreateReview([FromBody] ReviewCreateDTO dto)
        {
            var userId = User.GetUserId();
            var role = User.GetRole();
            var reviewDto = await _reviewService.AddReviewAsync((Guid)userId, dto.GameId, dto.Text, dto.Rating, userId, role);
            return CreatedAtAction(nameof(GetReviewsByGame), new { gameId = dto.GameId }, new SingleResponse<ReviewResponseDTO>
            {
                Item = new ReviewResponseDTO
                {
                    Id = reviewDto.Id,
                    UserId = reviewDto.UserId,
                    GameId = reviewDto.GameId,
                    Text = reviewDto.Text,
                    Rating = reviewDto.Rating,
                    CreationDate = reviewDto.CreationDate
                },
                Message = "Review created successfully"
            });
        }

        /// <summary>
        /// Retrieves a paginated list of reviews for a game.
        /// </summary>
        /// <param name="gameId">The ID of the game to retrieve reviews for.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of reviews per page (default is 10).</param>
        /// <param name="userId">The ID of the user to filter reviews by (optional).</param>
        /// <param name="minRating">The minimum rating to filter reviews by (optional).</param>
        /// <param name="maxRating">The maximum rating to filter reviews by (optional).</param>
        /// <returns>A paginated list of reviews.</returns>
        /// <response code="200">Returns the paginated list of reviews.</response>
        /// <response code="500">If an error occurs while retrieving reviews.</response>
        [HttpGet("game")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<ReviewResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetReviewsByGame(Guid gameId, int page = 1, int limit = 10, Guid? userId = null, int? minRating = null, int? maxRating = null)
        {
            try
            {
                var role = User.GetRole();
                var reviews = await _reviewService.GetReviewsByGameIdAsync(gameId, role, userId, minRating, maxRating);
                var totalCount = reviews.Count;
                var pagedReviews = reviews.Skip((page - 1) * limit).Take(limit).Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    GameId = r.GameId,
                    Text = r.Text,
                    Rating = r.Rating,
                    CreationDate = r.CreationDate,
                    Username = r.Username
                }).ToList();
                return Ok(new PaginatedResponse<ReviewResponseDTO>
                {
                    TotalCount = totalCount,
                    Items = pagedReviews,
                    NextPage = pagedReviews.Count == limit ? page + 1 : null,
                    PreviousPage = page > 1 ? page - 1 : null,
                    CurrentPage = page,
                    PageSize = limit
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve reviews", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a paginated list of reviews by a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose reviews are to be retrieved.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of reviews per page (default is 10).</param>
        /// <param name="sortByRating">The sort order for ratings ('asc' or 'desc', optional).</param>
        /// <returns>A paginated list of reviews.</returns>
        /// <response code="200">Returns the paginated list of reviews.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="500">If an error occurs while retrieving reviews.</response>
        [HttpGet("users/{userId}/reviews")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<ReviewResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetReviewsByUser(Guid userId, int page = 1, int limit = 10, string? sortByRating = null)
        {
            try
            {
                var currentUserId = User.GetUserId();
                var role = User.GetRole();
                if (userId != currentUserId && role != UserRole.Admin)
                    return Forbid("Only admins can view other users' reviews");
                var reviews = await _reviewService.GetByUserIdAsync(userId, sortByRating, role);
                var totalCount = reviews.Count;
                var pagedReviews = reviews.Skip((page - 1) * limit).Take(limit).Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    GameId = r.GameId,
                    Text = r.Text,
                    Rating = r.Rating,
                    CreationDate = r.CreationDate
                }).ToList();
                return Ok(new PaginatedResponse<ReviewResponseDTO>
                {
                    TotalCount = totalCount,
                    Items = pagedReviews,
                    NextPage = pagedReviews.Count == limit ? page + 1 : null,
                    PreviousPage = page > 1 ? page - 1 : null,
                    CurrentPage = page,
                    PageSize = limit
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve reviews", details = ex.Message });
            }
        }
    }
}