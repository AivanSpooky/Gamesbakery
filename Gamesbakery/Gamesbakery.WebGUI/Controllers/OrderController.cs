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
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;

        public OrderController(IOrderService orderService, ICartService cartService, IConfiguration configuration)
            : base(Log.ForContext<OrderController>(), configuration)
        {
            _orderService = orderService;
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    LogError(new UnauthorizedAccessException("User not authenticated"), "User not authenticated for order index");
                    return RedirectToAction("Login", "Account");
                }
                var orders = await _orderService.GetOrdersByUserIdAsync(userId.Value, GetCurrentRole());
                var ordersResponse = orders.Select(o => new OrderListResponseDTO
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    IsCompleted = o.IsCompleted,
                    IsOverdue = o.IsOverdue
                }).ToList();
                if (!ordersResponse.Any())
                {
                    ViewBag.Message = "У вас нет заказов.";
                }
                return View(ordersResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading orders for UserId={UserId}", GetCurrentUserId());
                ViewBag.ErrorMessage = $"Ошибка загрузки заказов: {ex.Message}";
                return View(new List<OrderListResponseDTO>());
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            try
            {
                if (userId == null)
                {
                    LogError(new UnauthorizedAccessException("User not authenticated"), "User not authenticated for order details");
                    return RedirectToAction("Login", "Account");
                }
                var order = await _orderService.GetOrderByIdAsync(id, userId, role);
                if (order == null || (order.UserId != userId && GetCurrentRole() != UserRole.Admin))
                {
                    LogError(new KeyNotFoundException($"Order {id} not found or access denied"), "Order not found or access denied");
                    return NotFound();
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
                        SellerName = oi.SellerName
                    }).ToList()
                };
                return View(orderResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading order details {Id}", id);
                ViewBag.ErrorMessage = $"Ошибка загрузки деталей заказа: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            try
            {
                if (userId == null)
                {
                    LogError(new UnauthorizedAccessException("User not authenticated"), "User not authenticated for checkout");
                    return RedirectToAction("Login", "Account");
                }
                var cartItems = await _cartService.GetCartItemsAsync(userId);
                var orderItemIds = cartItems.Select(ci => ci.OrderItemId).ToList();
                if (!orderItemIds.Any())
                {
                    TempData["ErrorMessage"] = "Корзина пуста.";
                    return RedirectToAction("Index", "Cart");
                }
                var order = await _orderService.CreateOrderAsync(userId.Value, orderItemIds, userId, role);
                await _cartService.ClearCartAsync(userId);
                TempData["SuccessMessage"] = "Заказ успешно оформлен!";
                return RedirectToAction("Details", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error creating order for UserId={UserId}", GetCurrentUserId());
                TempData["ErrorMessage"] = $"Ошибка при оформлении заказа: {ex.Message}";
                return RedirectToAction("Index", "Cart");
            }
        }
    }
}