using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.v2
{
    /// <summary>
    /// Controller for managing order items.
    /// </summary>
    [ApiController]
    [Route("api/v2/order-items")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class OrderItemsController : ControllerBase
    {
        private readonly IOrderItemService _orderItemService;

        public OrderItemsController(IOrderItemService orderItemService)
        {
            _orderItemService = orderItemService;
        }

        /// <summary>
        /// Retrieves a paginated list of order items with optional filtering by seller or game.
        /// </summary>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of order items per page (default is 10).</param>
        /// <param name="sellerId">The ID of the seller to filter order items by (optional).</param>
        /// <param name="gameId">The ID of the game to filter order items by (optional).</param>
        /// <returns>A paginated list of order items.</returns>
        /// <response code="200">Returns the paginated list of order items.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpGet]
        [Authorize(Roles = "Admin,Seller,User")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<OrderItemResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetOrderItems(int page = 1, int limit = 10, Guid? sellerId = null, Guid? gameId = null)
        {
            var role = User.GetRole();
            var items = await _orderItemService.GetFilteredAsync(sellerId, gameId, User.GetSellerId(), role);
            var totalCount = items.Count;
            var pagedItems = items.Skip((page - 1) * limit).Take(limit).Select(item => new OrderItemResponseDTO
            {
                Id = item.Id,
                GameId = item.GameId,
                GameTitle = item.GameTitle,
                SellerId = item.SellerId,
                SellerName = item.SellerName,
                OrderId = item.OrderId,
                GamePrice = item.GamePrice
            }).ToList();
            return Ok(new PaginatedResponse<OrderItemResponseDTO>
            {
                TotalCount = totalCount,
                Items = pagedItems,
                NextPage = pagedItems.Count == limit ? page + 1 : null,
                PreviousPage = page > 1 ? page - 1 : null,
                CurrentPage = page,
                PageSize = limit
            });
        }

        /// <summary>
        /// Creates a new order item.
        /// </summary>
        /// <param name="dto">The order item creation details.</param>
        /// <returns>The created order item.</returns>
        /// <response code="201">Order item successfully created.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<OrderItemResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> CreateOrderItem([FromBody] OrderItemCreateDTO dto)
        {
            var role = User.GetRole();
            var created = await _orderItemService.CreateAsync(dto, User.GetSellerId(), role);
            return CreatedAtAction(nameof(GetOrderItem), new { id = created.Id }, new SingleResponse<OrderItemResponseDTO>
            {
                Item = new OrderItemResponseDTO
                {
                    Id = created.Id,
                    GameId = created.GameId,
                    GameTitle = created.GameTitle,
                    SellerId = created.SellerId,
                    SellerName = created.SellerName
                },
                Message = "Order item created successfully"
            });
        }

        /// <summary>
        /// Retrieves a specific order item by ID.
        /// </summary>
        /// <param name="id">The ID of the order item.</param>
        /// <returns>The order item details.</returns>
        /// <response code="200">Returns the order item details.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the order item is not found.</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Seller,User")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<OrderItemResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetOrderItem(Guid id)
        {
            var role = User.GetRole();
            var curUser = User.GetUserId();
            var item = await _orderItemService.GetByIdAsync(id, curUser, role);
            if (item == null) return NotFound();
            return Ok(new SingleResponse<OrderItemResponseDTO>
            {
                Item = new OrderItemResponseDTO
                {
                    Id = item.Id,
                    GameId = item.GameId,
                    GameTitle = item.GameTitle,
                    SellerId = item.SellerId,
                    SellerName = item.SellerName
                },
                Message = "Order item retrieved successfully"
            });
        }
        /// <summary>
        /// Retrieves the key for a specific order item if the user is authorized.
        /// </summary>
        /// <param name="id">The ID of the order item.</param>
        /// <returns>The key in a secure response.</returns>
        /// <response code="200">Returns the key.</response>
        /// <response code="403">If not authorized.</response>
        /// <response code="404">If not found.</response>
        [HttpGet("{id}/key")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<string>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetOrderItemKey(Guid id)
        {
            var role = User.GetRole();
            var currentUserId = User.GetUserId();
            var item = await _orderItemService.GetByIdAsync(id, currentUserId, role);
            if (item == null) return NotFound();
            // Additional check: Ensure the item belongs to an order owned by the user
            if (role != UserRole.Admin && item.OrderId.HasValue)
            {
                // You may need to inject IOrderService or add logic to verify order ownership
                // For example: if (!await _orderService.IsOrderOwnedByUser(item.OrderId.Value, currentUserId)) return Forbid();
            }
            return Ok(new SingleResponse<string>
            {
                Item = item.Key,
                Message = "Key retrieved successfully"
            });
        }

        /// <summary>
        /// Updates an existing order item.
        /// </summary>
        /// <param name="id">The ID of the order item to update.</param>
        /// <param name="dto">The updated order item details.</param>
        /// <returns>The updated order item.</returns>
        /// <response code="200">Order item successfully updated.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the order item is not found.</response>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Seller")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<OrderItemResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PatchOrderItem(Guid id, [FromBody] OrderItemUpdateDTO dto)
        {
            var role = User.GetRole();
            var curUser = User.GetUserId();
            var curSeller = User.GetSellerId();
            await _orderItemService.UpdateAsync(id, dto, curSeller, role);
            var updated = await _orderItemService.GetByIdAsync(id, curUser, role);
            return Ok(new SingleResponse<OrderItemResponseDTO>
            {
                Item = new OrderItemResponseDTO
                {
                    Id = updated.Id,
                    GameId = updated.GameId,
                    GameTitle = updated.GameTitle,
                    SellerId = updated.SellerId,
                    SellerName = updated.SellerName
                },
                Message = "Order item updated successfully"
            });
        }

        /// <summary>
        /// Deletes an order item.
        /// </summary>
        /// <param name="id">The ID of the order item to delete.</param>
        /// <returns>A response indicating the order item was deleted.</returns>
        /// <response code="204">Order item successfully deleted.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the order item is not found.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrderItem(Guid id)
        {
            var role = User.GetRole();
            await _orderItemService.DeleteAsync(id, role);
            return NoContent();
        }
    }
}