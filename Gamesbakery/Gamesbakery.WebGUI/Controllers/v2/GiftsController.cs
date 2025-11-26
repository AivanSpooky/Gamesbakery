using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.V2
{
    /// <summary>
    /// Controller for managing user gifts.
    /// </summary>
    [ApiController]
    [Route("api/v2")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class GiftsController : ControllerBase
    {
        private readonly IGiftService giftService;

        public GiftsController(IGiftService giftService)
        {
            this.giftService = giftService;
        }

        /// <summary>
        /// Retrieves a paginated list of gifts for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose gifts are to be retrieved.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of gifts per page (default is 10).</param>
        /// <param name="type">The type of gifts to retrieve: 'sent', 'received', or 'all' (default is 'all').</param>
        /// <returns>A paginated list of gifts.</returns>
        /// <response code="200">Returns the paginated list of gifts.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpGet("users/{userId}/gifts")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<GiftResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetUserGifts(Guid userId, int page = 1, int limit = 10, string type = "all")
        {
            var currentUserId = this.User.GetUserId();
            var role = this.User.GetRole();
            if (userId != currentUserId && role != UserRole.Admin) return this.Forbid();
            var gifts = type switch
            {
                "sent" => await this.giftService.GetGiftsBySenderAsync(userId, currentUserId, role),
                "received" => await this.giftService.GetGiftsByRecipientAsync(userId, currentUserId, role),
                _ => (await this.giftService.GetGiftsBySenderAsync(userId, currentUserId, role)).Concat(await this.giftService.GetGiftsByRecipientAsync(userId, currentUserId, role))
            };
            var totalCount = gifts.Count();
            var pagedGifts = gifts.Skip((page - 1) * limit).Take(limit).Select(g => new GiftResponseDTO
            {
                GiftId = g.GiftId,
                SenderId = g.SenderId,
                RecipientId = g.RecipientId,
                OrderItemId = g.OrderItemId,
                GiftDate = g.GiftDate,
                GameTitle = g.GameTitle,
            }).ToList();
            return this.Ok(new PaginatedResponse<GiftResponseDTO>
            {
                TotalCount = totalCount,
                Items = pagedGifts,
                NextPage = pagedGifts.Count == limit ? page + 1 : null,
                PreviousPage = page > 1 ? page - 1 : null,
                CurrentPage = page,
                PageSize = limit,
            });
        }

        /// <summary>
        /// Sends a gift to another user.
        /// </summary>
        /// <param name="userId">The ID of the sender.</param>
        /// <param name="dto">The gift creation details.</param>
        /// <returns>The created gift.</returns>
        /// <response code="201">Gift successfully sent.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpPost("users/{userId}/gifts")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<GiftResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> SendGift(Guid userId, [FromBody] GiftCreateDTO dto)
        {
            var currentUserId = this.User.GetUserId();
            var role = this.User.GetRole();
            if (userId != currentUserId) return this.Forbid();
            var gift = await this.giftService.SendGiftAsync(userId, dto.RecipientId, dto.OrderItemId, currentUserId, role);
            return this.CreatedAtAction(nameof(this.GetGift), new { id = gift.GiftId }, new SingleResponse<GiftResponseDTO>
            {
                Item = new GiftResponseDTO
                {
                    GiftId = gift.GiftId,
                    SenderId = gift.SenderId,
                    RecipientId = gift.RecipientId,
                    OrderItemId = gift.OrderItemId,
                    GiftDate = gift.GiftDate,
                    GameTitle = gift.GameTitle,
                },
                Message = "Gift sent successfully",
            });
        }

        /// <summary>
        /// Retrieves a specific gift by ID.
        /// </summary>
        /// <param name="id">The ID of the gift.</param>
        /// <returns>The gift details.</returns>
        /// <response code="200">Returns the gift details.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the gift is not found.</response>
        [HttpGet("gifts/{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<GiftResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetGift(Guid id)
        {
            var role = this.User.GetRole();
            var currentUserId = this.User.GetUserId();
            var gift = await this.giftService.GetGiftByIdAsync(id, currentUserId, role);
            if (gift == null) return this.NotFound();
            if ((gift.SenderId != currentUserId && gift.RecipientId != currentUserId) && role != UserRole.Admin) return this.Forbid();
            return this.Ok(new SingleResponse<GiftResponseDTO>
            {
                Item = new GiftResponseDTO
                {
                    GiftId = gift.GiftId,
                    SenderId = gift.SenderId,
                    RecipientId = gift.RecipientId,
                    OrderItemId = gift.OrderItemId,
                    GiftDate = gift.GiftDate,
                    GameTitle = gift.GameTitle,
                },
                Message = "Gift retrieved successfully",
            });
        }

        /// <summary>
        /// Deletes a gift.
        /// </summary>
        /// <param name="id">The ID of the gift to delete.</param>
        /// <returns>A response indicating the gift was deleted.</returns>
        /// <response code="204">Gift successfully deleted.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpDelete("gifts/{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> DeleteGift(Guid id)
        {
            var role = this.User.GetRole();
            await this.giftService.DeleteGiftAsync(id, role);
            return this.NoContent();
        }
    }
}
