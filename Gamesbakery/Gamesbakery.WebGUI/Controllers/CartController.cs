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
        private readonly ICartService cartService;
        private readonly IOrderService orderService;

        public CartController(ICartService cartService, IOrderService orderService, IConfiguration configuration)
            : base(Log.ForContext<CartController>(), configuration)
        {
            this.cartService = cartService;
            this.orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = this.User.GetUserId();
            try
            {
                var cartItems = await this.cartService.GetCartItemsAsync(currentUserId);
                var cartItemsResponse = cartItems.Select(item => new CartItemResponseDTO
                {
                    OrderItemId = item.OrderItemId,
                    GameId = item.GameId,
                    GameTitle = item.GameTitle,
                    GamePrice = item.GamePrice,
                    SellerName = item.SellerName,
                }).ToList();
                this.ViewBag.Total = await this.cartService.GetCartTotalAsync(currentUserId);
                return this.View(cartItemsResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading cart");
                return this.View("Error", new ErrorViewModel { ErrorMessage = $"Ошибка загрузки корзины: {ex.Message}" });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Add(Guid orderItemId)
        {
            var currentUserId = this.User.GetUserId();
            try
            {
                await this.cartService.AddToCartAsync(orderItemId, currentUserId);
                this.TempData["SuccessMessage"] = "Товар добавлен в корзину.";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error adding item to cart {OrderItemId}", orderItemId);
                this.TempData["ErrorMessage"] = $"Ошибка при добавлении в корзину: {ex.Message}";
                return this.RedirectToAction("Index", "Game");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Remove(Guid orderItemId)
        {
            var currentUserId = this.User.GetUserId();
            try
            {
                await this.cartService.RemoveFromCartAsync(orderItemId, currentUserId);
                this.TempData["SuccessMessage"] = "Товар удален из корзины.";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error removing item from cart {OrderItemId}", orderItemId);
                this.TempData["ErrorMessage"] = $"Ошибка при удалении из корзины: {ex.Message}";
                return this.RedirectToAction("Index");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Clear()
        {
            var currentUserId = this.User.GetUserId();
            try
            {
                await this.cartService.ClearCartAsync(currentUserId);
                this.TempData["SuccessMessage"] = "Корзина очищена.";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error clearing cart");
                this.TempData["ErrorMessage"] = $"Ошибка при очистке корзины: {ex.Message}";
                return this.RedirectToAction("Index");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var currentUserId = this.User.GetUserId();
            try
            {
                var userId = this.GetCurrentUserId();
                if (userId == null)
                    return this.RedirectToAction("Login", "Account");
                var cartItems = await this.cartService.GetCartItemsAsync(currentUserId);
                var orderItemIds = cartItems.Select(ci => ci.OrderItemId).ToList();
                if (!orderItemIds.Any())
                {
                    this.TempData["ErrorMessage"] = "Корзина пуста.";
                    return this.RedirectToAction("Index");
                }

                var order = await this.orderService.CreateOrderAsync(userId.Value, orderItemIds, userId, this.GetCurrentRole());
                await this.cartService.ClearCartAsync(currentUserId);
                this.TempData["SuccessMessage"] = "Заказ успешно оформлен!";
                return this.RedirectToAction("Details", "Order", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error creating order");
                this.TempData["ErrorMessage"] = $"Ошибка при оформлении заказа: {ex.Message}";
                return this.RedirectToAction("Index");
            }
        }
    }
}
