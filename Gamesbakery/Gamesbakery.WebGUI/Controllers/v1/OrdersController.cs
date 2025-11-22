using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/orders")]
    [Authorize(Roles = "User")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderV1DTO request)
        {
            try
            {
                var userId = User.GetUserId();
                var role = User.GetRole();

                var order = await _orderService.CreateOrderAsync(userId.Value, request.GameIds, userId, role);
                return Ok(new { orderId = order.OrderId, message = "Order created" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var userId = User.GetUserId();
                var role = User.GetRole();

                var orders = await _orderService.GetOrdersByUserIdAsync(userId.Value, role);
                return Ok(new { orders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get orders" });
            }
        }
    }

    public class CreateOrderV1DTO
    {
        public List<Guid> GameIds { get; set; } = new List<Guid>();
    }
}