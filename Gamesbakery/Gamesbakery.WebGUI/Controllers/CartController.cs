using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.CartDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class CartController : BaseController
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;

        public CartController(ICartService cartService, IOrderService orderService, IConfiguration configuration)
            : base(Log.ForContext<CartController>(), configuration)
        {
            _cartService = cartService;
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = User.GetUserId();
            try
            {
                var cartItems = await _cartService.GetCartItemsAsync(currentUserId);
                var cartItemsResponse = cartItems.Select(item => new CartItemResponseDTO
                {
                    OrderItemId = item.OrderItemId,
                    GameId = item.GameId,
                    GameTitle = item.GameTitle,
                    GamePrice = item.GamePrice,
                    SellerName = item.SellerName
                }).ToList();
                ViewBag.Total = await _cartService.GetCartTotalAsync(currentUserId);
                return View(cartItemsResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading cart");
                return View("Error", new ErrorViewModel { ErrorMessage = $"Ошибка загрузки корзины: {ex.Message}" });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Add(Guid orderItemId)
        {
            var currentUserId = User.GetUserId();
            try
            {
                await _cartService.AddToCartAsync(orderItemId, currentUserId);
                TempData["SuccessMessage"] = "Товар добавлен в корзину.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error adding item to cart {OrderItemId}", orderItemId);
                TempData["ErrorMessage"] = $"Ошибка при добавлении в корзину: {ex.Message}";
                return RedirectToAction("Index", "Game");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Remove(Guid orderItemId)
        {
            var currentUserId = User.GetUserId();
            try
            {
                await _cartService.RemoveFromCartAsync(orderItemId, currentUserId);
                TempData["SuccessMessage"] = "Товар удален из корзины.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error removing item from cart {OrderItemId}", orderItemId);
                TempData["ErrorMessage"] = $"Ошибка при удалении из корзины: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Clear()
        {
            var currentUserId = User.GetUserId();
            try
            {
                await _cartService.ClearCartAsync(currentUserId);
                TempData["SuccessMessage"] = "Корзина очищена.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error clearing cart");
                TempData["ErrorMessage"] = $"Ошибка при очистке корзины: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var currentUserId = User.GetUserId();
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return RedirectToAction("Login", "Account");
                var cartItems = await _cartService.GetCartItemsAsync(currentUserId);
                var orderItemIds = cartItems.Select(ci => ci.OrderItemId).ToList();
                if (!orderItemIds.Any())
                {
                    TempData["ErrorMessage"] = "Корзина пуста.";
                    return RedirectToAction("Index");
                }
                var order = await _orderService.CreateOrderAsync(userId.Value, orderItemIds, userId, GetCurrentRole());
                await _cartService.ClearCartAsync(currentUserId);
                TempData["SuccessMessage"] = "Заказ успешно оформлен!";
                return RedirectToAction("Details", "Order", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error creating order");
                TempData["ErrorMessage"] = $"Ошибка при оформлении заказа: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}