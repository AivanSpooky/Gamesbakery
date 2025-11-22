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
        private readonly ISellerService _sellerService;
        private readonly IGameService _gameService;

        public SellerController(ISellerService sellerService, IGameService gameService, IConfiguration configuration)
            : base(Log.ForContext<SellerController>(), configuration)
        {
            _sellerService = sellerService;
            _gameService = gameService;
        }

        public async Task<IActionResult> Profile()
        {
            try
            {
                var role = GetCurrentRole();
                var sellerId = GetCurrentSellerId();
                if (sellerId == null)
                    return RedirectToAction("Login", "Account");
                var seller = await _sellerService.GetSellerByIdAsync(sellerId.Value, sellerId, role);
                if (seller == null)
                    return NotFound();
                var sellerResponse = new SellerResponseDTO
                {
                    Id = seller.Id,
                    SellerName = seller.SellerName,
                    RegistrationDate = seller.RegistrationDate,
                    AvgRating = seller.AvgRating
                };
                return View(sellerResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error retrieving seller profile");
                ViewBag.ErrorMessage = $"Ошибка загрузки профиля: {ex.Message}";
                return View();
            }
        }

        public async Task<IActionResult> OrderItems()
        {
            try
            {
                var role = GetCurrentRole();
                var sellerId = GetCurrentSellerId();
                if (sellerId == null)
                    return RedirectToAction("Login", "Account");
                var games = await _gameService.GetAllGamesAsync();
                var orderItems = await _sellerService.GetOrderItemsBySellerIdAsync(sellerId.Value, sellerId, role);
                ViewBag.Games = games.Select(g => new GameListResponseDTO
                {
                    Id = g.Id,
                    Title = g.Title,
                    Price = g.Price,
                    IsForSale = g.IsForSale
                }).ToList();
                var orderItemsResponse = orderItems.Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    GameId = oi.GameId,
                    GameTitle = oi.GameTitle,
                    SellerId = oi.SellerId,
                    SellerName = oi.SellerName
                }).ToList();
                return View(orderItemsResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error retrieving seller order items");
                ViewBag.ErrorMessage = $"Ошибка загрузки товаров: {ex.Message}";
                return View(new List<OrderItemResponseDTO>());
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateKey(Guid gameId, string key)
        {
            try
            {
                var role = GetCurrentRole();
                var sellerId = GetCurrentSellerId();
                if (sellerId == null)
                    return RedirectToAction("Login", "Account");
                if (string.IsNullOrWhiteSpace(key))
                {
                    TempData["ErrorMessage"] = "Ключ не может быть пустым.";
                    return RedirectToAction("OrderItems");
                }
                var orderItem = await _sellerService.CreateKeyAsync(gameId, key, sellerId, role);
                TempData["SuccessMessage"] = "Ключ успешно создан!";
                return RedirectToAction("OrderItems");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error creating key for game {GameId}", gameId);
                TempData["ErrorMessage"] = $"Ошибка создания ключа: {ex.Message}";
                return RedirectToAction("OrderItems");
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SetOrderItemKey(Guid orderItemId, string key)
        {
            try
            {
                var role = GetCurrentRole();
                var sellerId = GetCurrentSellerId();
                if (sellerId == null)
                    return RedirectToAction("Login", "Account");
                if (string.IsNullOrWhiteSpace(key))
                {
                    TempData["ErrorMessage"] = "Ключ не может быть пустым.";
                    return RedirectToAction("OrderItems");
                }
                await _sellerService.SetOrderItemKeyAsync(orderItemId, key, sellerId.Value, sellerId, role);
                TempData["SuccessMessage"] = "Ключ успешно установлен!";
                return RedirectToAction("OrderItems");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error setting key for order item {OrderItemId}", orderItemId);
                TempData["ErrorMessage"] = $"Ошибка установки ключа: {ex.Message}";
                return RedirectToAction("OrderItems");
            }
        }
    }
}