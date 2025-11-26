using System;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    [Authorize(Roles = "Seller,Admin")]
    public class SellerController : BaseController
    {
        private readonly ISellerService sellerService;
        private readonly IGameService gameService;

        public SellerController(ISellerService sellerService, IGameService gameService, IConfiguration configuration)
            : base(Log.ForContext<SellerController>(), configuration)
        {
            this.sellerService = sellerService;
            this.gameService = gameService;
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                var role = this.GetCurrentRole();
                var sellerId = this.GetCurrentSellerId();
                if (sellerId == null)
                    return this.RedirectToAction("Login", "Account");
                var seller = await this.sellerService.GetSellerByIdAsync(sellerId.Value, sellerId, role);
                if (seller == null)
                    return this.NotFound();
                var sellerResponse = new SellerResponseDTO
                {
                    Id = seller.Id,
                    SellerName = seller.SellerName,
                    RegistrationDate = seller.RegistrationDate,
                    AvgRating = seller.AvgRating,
                };
                return this.View(sellerResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error retrieving seller profile");
                this.ViewBag.ErrorMessage = $"Ошибка загрузки профиля: {ex.Message}";
                return this.View();
            }
        }

        public async Task<IActionResult> OrderItems()
        {
            try
            {
                var role = this.GetCurrentRole();
                var sellerId = this.GetCurrentSellerId();
                if (sellerId == null)
                    return this.RedirectToAction("Login", "Account");
                var games = await this.gameService.GetAllGamesAsync();
                var orderItems = await this.sellerService.GetOrderItemsBySellerIdAsync(sellerId.Value, sellerId, role);
                this.ViewBag.Games = games.Select(g => new GameListResponseDTO
                {
                    Id = g.Id,
                    Title = g.Title,
                    Price = g.Price,
                    IsForSale = g.IsForSale,
                }).ToList();
                var orderItemsResponse = orderItems.Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    GameId = oi.GameId,
                    GameTitle = oi.GameTitle,
                    SellerId = oi.SellerId,
                    SellerName = oi.SellerName,
                }).ToList();
                return this.View(orderItemsResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error retrieving seller order items");
                this.ViewBag.ErrorMessage = $"Ошибка загрузки товаров: {ex.Message}";
                return this.View(new List<OrderItemResponseDTO>());
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateKey(Guid gameId, string key)
        {
            try
            {
                var role = this.GetCurrentRole();
                var sellerId = this.GetCurrentSellerId();
                if (sellerId == null)
                    return this.RedirectToAction("Login", "Account");
                if (string.IsNullOrWhiteSpace(key))
                {
                    this.TempData["ErrorMessage"] = "Ключ не может быть пустым.";
                    return this.RedirectToAction("OrderItems");
                }

                var orderItem = await this.sellerService.CreateKeyAsync(gameId, key, sellerId, role);
                this.TempData["SuccessMessage"] = "Ключ успешно создан!";
                return this.RedirectToAction("OrderItems");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error creating key for game {GameId}", gameId);
                this.TempData["ErrorMessage"] = $"Ошибка создания ключа: {ex.Message}";
                return this.RedirectToAction("OrderItems");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SetOrderItemKey(Guid orderItemId, string key)
        {
            try
            {
                var role = this.GetCurrentRole();
                var sellerId = this.GetCurrentSellerId();
                if (sellerId == null)
                    return this.RedirectToAction("Login", "Account");
                if (string.IsNullOrWhiteSpace(key))
                {
                    this.TempData["ErrorMessage"] = "Ключ не может быть пустым.";
                    return this.RedirectToAction("OrderItems");
                }

                await this.sellerService.SetOrderItemKeyAsync(orderItemId, key, sellerId.Value, sellerId, role);
                this.TempData["SuccessMessage"] = "Ключ успешно установлен!";
                return this.RedirectToAction("OrderItems");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error setting key for order item {OrderItemId}", orderItemId);
                this.TempData["ErrorMessage"] = $"Ошибка установки ключа: {ex.Message}";
                return this.RedirectToAction("OrderItems");
            }
        }
    }
}
