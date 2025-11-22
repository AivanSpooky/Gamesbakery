using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Core.Repositories;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.v2
{
    /// <summary>
    /// Controller for managing user orders.
    /// </summary>
    [ApiController]
    [Route("api/v2")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrdersController(IOrderRepository orderRepository, IOrderService orderService, ICartService cartService)
        {
            _orderRepository = orderRepository;
            _orderService = orderService;
            _cartService = cartService;
        }

        /// <summary>
        /// Retrieves a paginated list of orders for a user.
        /// </summary>
        /// <param name="userId">The ID of the user whose orders are to be retrieved.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="limit">The number of orders per page (default is 10).</param>
        /// <returns>A paginated list of orders.</returns>
        /// <response code="200">Returns the paginated list of orders.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpGet("users/{userId}/orders")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<OrderListResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetUserOrders(Guid userId, int page = 1, int limit = 10)
        {
            var currentUserId = User.GetUserId();
            var role = User.GetRole();
            if (userId != currentUserId && role != UserRole.Admin) return Forbid();
            var orders = await _orderService.GetOrdersByUserIdAsync(userId, role);
            var totalCount = orders.Count;
            var pagedOrders = orders.Skip((page - 1) * limit).Take(limit).Select(o => new OrderListResponseDTO
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                IsCompleted = o.IsCompleted,
                IsOverdue = o.IsOverdue
            }).ToList();
            return Ok(new PaginatedResponse<OrderListResponseDTO>
            {
                TotalCount = totalCount,
                Items = pagedOrders,
                NextPage = pagedOrders.Count == limit ? page + 1 : null,
                PreviousPage = page > 1 ? page - 1 : null,
                CurrentPage = page,
                PageSize = limit
            });
        }

        /// <summary>
        /// Creates a new order for a user.
        /// </summary>
        /// <param name="userId">The ID of the user creating the order.</param>
        /// <param name="dto">The order creation details.</param>
        /// <returns>The created order.</returns>
        /// <response code="201">Order successfully created.</response>
        /// <response code="400">If the request data is invalid.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        [HttpPost("users/{userId}/orders")]
        [Authorize(Roles = "User")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(SingleResponse<OrderListResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> CreateOrder(Guid userId, [FromBody] OrderCreateDTO dto)
        {
            try
            {
                var role = User.GetRole();
                var currentUserId = User.GetUserId();
                if (userId != currentUserId)
                    return Forbid("Can only create orders for yourself");
                var orderDetails = await _orderService.CreateOrderAsync(userId, dto.CartItemIds, currentUserId, role);
                return CreatedAtAction(
                    nameof(GetOrder),
                    new { id = orderDetails.OrderId },
                    new SingleResponse<OrderListResponseDTO>
                    {
                        Item = new OrderListResponseDTO
                        {
                            OrderId = orderDetails.OrderId,
                            OrderDate = orderDetails.OrderDate,
                            TotalAmount = orderDetails.TotalAmount,
                            IsCompleted = orderDetails.IsCompleted,
                            IsOverdue = orderDetails.IsOverdue
                        },
                        Message = "Order created successfully"
                    });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create order", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific order by ID.
        /// </summary>
        /// <param name="id">The ID of the order.</param>
        /// <returns>The order details.</returns>
        /// <response code="200">Returns the order details.</response>
        /// <response code="403">If the requesting user is not authorized.</response>
        /// <response code="404">If the order is not found.</response>
        [HttpGet("orders/{id}")]
        [Authorize(Roles = "User,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<OrderDetailsResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetOrder(Guid id)
        {
            var role = User.GetRole();
            var currentUserId = User.GetUserId();
            var order = await _orderService.GetOrderByIdAsync(id, currentUserId, role);
            if (order == null) return NotFound();
            if (order.UserId != currentUserId && role != UserRole.Admin) return Forbid();
            return Ok(new SingleResponse<OrderDetailsResponseDTO>
            {
                Item = new OrderDetailsResponseDTO
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    OrderDate = order.OrderDate,
                    TotalPrice = order.TotalPrice,
                    IsCompleted = order.IsCompleted,
                    IsOverdue = order.IsOverdue,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDTO
                    {
                        Id = oi.Id,
                        GameId = oi.GameId,
                        GameTitle = oi.GameTitle,
                        SellerId = oi.SellerId,
                        SellerName = oi.SellerName
                    }).ToList()
                },
                Message = "Order retrieved successfully"
            });
        }
    }
}