using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class OrderController : BaseController
    {
        private readonly IOrderService orderService;
        private readonly ICartService cartService;

        public OrderController(IOrderService orderService, ICartService cartService, IConfiguration configuration)
            : base(Log.ForContext<OrderController>(), configuration)
        {
            this.orderService = orderService;
            this.cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = this.GetCurrentUserId();
                if (userId == null)
                {
                    this.LogError(new UnauthorizedAccessException("User not authenticated"), "User not authenticated for order index");
                    return this.RedirectToAction("Login", "Account");
                }

                var orders = await this.orderService.GetOrdersByUserIdAsync(userId.Value, this.GetCurrentRole());
                var ordersResponse = orders.Select(o => new OrderListResponseDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    IsCompleted = o.IsCompleted,
                    IsOverdue = o.IsOverdue,
                }).ToList();
                if (!ordersResponse.Any())
                    this.ViewBag.Message = "У вас нет заказов.";
                return this.View(ordersResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading orders for UserId={UserId}", this.GetCurrentUserId());
                this.ViewBag.ErrorMessage = $"Ошибка загрузки заказов: {ex.Message}";
                return this.View(new List<OrderListResponseDTO>());
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var role = this.GetCurrentRole();
            var userId = this.GetCurrentUserId();
            try
            {
                if (userId == null)
                {
                    this.LogError(new UnauthorizedAccessException("User not authenticated"), "User not authenticated for order details");
                    return this.RedirectToAction("Login", "Account");
                }

                var order = await this.orderService.GetOrderByIdAsync(id, userId, role);
                if (order == null || (order.UserId != userId && this.GetCurrentRole() != UserRole.Admin))
                {
                    this.LogError(new KeyNotFoundException($"Order {id} not found or access denied"), "Order not found or access denied");
                    return this.NotFound();
                }

                var orderResponse = new OrderDetailsResponseDTO
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
                        SellerName = oi.SellerName,
                    }).ToList(),
                };
                return this.View(orderResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading order details {Id}", id);
                this.ViewBag.ErrorMessage = $"Ошибка загрузки деталей заказа: {ex.Message}";
                return this.View();
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var role = this.GetCurrentRole();
            var userId = this.GetCurrentUserId();
            try
            {
                if (userId == null)
                {
                    this.LogError(new UnauthorizedAccessException("User not authenticated"), "User not authenticated for checkout");
                    return this.RedirectToAction("Login", "Account");
                }

                var cartItems = await this.cartService.GetCartItemsAsync(userId);
                var orderItemIds = cartItems.Select(ci => ci.OrderItemId).ToList();
                if (!orderItemIds.Any())
                {
                    this.TempData["ErrorMessage"] = "Корзина пуста.";
                    return this.RedirectToAction("Index", "Cart");
                }

                var order = await this.orderService.CreateOrderAsync(userId.Value, orderItemIds, userId, role);
                await this.cartService.ClearCartAsync(userId);
                this.TempData["SuccessMessage"] = "Заказ успешно оформлен!";
                return this.RedirectToAction("Details", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error creating order for UserId={UserId}", this.GetCurrentUserId());
                this.TempData["ErrorMessage"] = $"Ошибка при оформлении заказа: {ex.Message}";
                return this.RedirectToAction("Index", "Cart");
            }
        }
    }
}
