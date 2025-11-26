using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.CartDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.V2
{
    /// <summary>
    /// Controller for managing user cart items.
    /// </summary>
    [ApiController]
    [Route("api/v2/users/{userId}/cart-items")]
    [Authorize(Roles = "User")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class CartsController : ControllerBase
    {
        private readonly ICartService cartService;

        public CartsController(ICartService cartService)
        {
            this.cartService = cartService;
        }

        /// <summary>
        /// Retrieves a paginated list of cart items for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose cart items are to be retrieved.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of items per page (default is 10).</param>
        /// <returns>A paginated list of cart items.</returns>
        /// <response code="200">Returns the paginated list of cart items.</response>
        /// <response code="403">If the requesting user is not authorized to view the cart.</response>
        [HttpGet]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<CartItemResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetCartItems(Guid userId, int page = 1, int limit = 10)
        {
            var currentUserId = this.User.GetUserId();
            if (userId != currentUserId) return this.Forbid();
            var items = await this.cartService.GetCartItemsAsync(currentUserId);
            var totalCount = items.Count;
            var pagedItems = items.Skip((page - 1) * limit).Take(limit).Select(item => new CartItemResponseDTO
            {
                OrderItemId = item.OrderItemId,
                GameId = item.GameId,
                GameTitle = item.GameTitle,
                GamePrice = item.GamePrice,
                SellerName = item.SellerName,
            }).ToList();
            return this.Ok(new PaginatedResponse<CartItemResponseDTO>
            {
                TotalCount = totalCount,
                Items = pagedItems,
                NextPage = pagedItems.Count == limit ? page + 1 : null,
                PreviousPage = page > 1 ? page - 1 : null,
                CurrentPage = page,
                PageSize = limit,
            });
        }

        /// <summary>
        /// Adds an item to the user's cart.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="dto">The item to add to the cart.</param>
        /// <returns>A response indicating the item was added.</returns>
        /// <response code="201">Item successfully added to the cart.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<object>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> AddItem(Guid userId, [FromBody] AddToCartDTO dto)
        {
            var currentUserId = this.User.GetUserId();
            if (userId != currentUserId) return this.Forbid();
            await this.cartService.AddToCartAsync(dto.OrderItemId, currentUserId);
            return this.CreatedAtAction(nameof(this.GetCartItems), new { userId }, new SingleResponse<object>
            {
                Item = null,
                Message = "Item added to cart",
            });
        }

        /// <summary>
        /// Removes an item from the user's cart.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="itemId">The ID of the item to remove.</param>
        /// <returns>A response indicating the item was removed.</returns>
        /// <response code="204">Item successfully removed from the cart.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpDelete("{itemId}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> RemoveItem(Guid userId, Guid itemId)
        {
            var currentUserId = this.User.GetUserId();
            if (userId != currentUserId) return this.Forbid();
            await this.cartService.RemoveFromCartAsync(itemId, currentUserId);
            return this.NoContent();
        }

        /// <summary>
        /// Clears all items from the user's cart.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A response indicating the cart was cleared.</returns>
        /// <response code="204">Cart successfully cleared.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpDelete]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ClearCart(Guid userId)
        {
            var currentUserId = this.User.GetUserId();
            if (userId != currentUserId) return this.Forbid();
            await this.cartService.ClearCartAsync(currentUserId);
            return this.NoContent();
        }
    }
}
