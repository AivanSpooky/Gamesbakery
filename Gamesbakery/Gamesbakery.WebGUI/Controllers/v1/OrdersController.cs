using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gamesbakery.WebGUI.Controllers.V1
{
    [ApiController]
    [Route("api/v1/orders")]
    [Authorize(Roles = "User")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService orderService;

        public OrdersController(IOrderService orderService)
        {
            this.orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderV1DTO request)
        {
            try
            {
                var userId = this.User.GetUserId();
                var role = this.User.GetRole();

                var order = await this.orderService.CreateOrderAsync(userId.Value, request.GameIds, userId, role);
                return this.Ok(new { orderId = order.OrderId, message = "Order created" });
            }
            catch (Exception ex)
            {
                return this.BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var userId = this.User.GetUserId();
                var role = this.User.GetRole();

                var orders = await this.orderService.GetOrdersByUserIdAsync(userId.Value, role);
                return this.Ok(new { orders });
            }
            catch (Exception)
            {
                return this.StatusCode(500, new { error = "Failed to get orders" });
            }
        }
    }
}
