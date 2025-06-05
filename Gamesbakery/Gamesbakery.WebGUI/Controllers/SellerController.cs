using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Gamesbakery.Infrastructure;
using Gamesbakery.WebGUI.Models;
using Gamesbakery.Core.DTOs;
using Serilog;
using System;
using System.Threading.Tasks;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.Controllers
{
    public class SellerController : BaseController
    {
        private readonly ISellerService _sellerService;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IGameService _gameService;
        private readonly IAuthenticationService _authService;

        public SellerController(
        ISellerService sellerService,
        IOrderItemRepository orderItemRepository,
        IGameService gameService,
        IAuthenticationService authService,
        IConfiguration configuration)
        : base(Log.ForContext<SellerController>(), configuration)
        {
            _sellerService = sellerService;
            _orderItemRepository = orderItemRepository;
            _gameService = gameService;
            _authService = authService;
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            using (PushLogContext())
            {
                try
                {
                    var sellerId = _authService.GetCurrentSellerId();
                    if (!sellerId.HasValue || _authService.GetCurrentRole() != UserRole.Seller)
                    {
                        LogWarning("Unauthorized access to seller profile");
                        return Unauthorized("Only sellers can access this page.");
                    }

                    var seller = await _sellerService.GetSellerByIdAsync(sellerId.Value);
                    LogInformation("Retrieved profile for seller ID: {SellerId}", sellerId);
                    return View(seller);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving seller profile");
                    ViewBag.ErrorMessage = "Произошла ошибка при загрузке профиля.";
                    return View();
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> OrderItems()
        {
            using (PushLogContext())
            {
                try
                {
                    var sellerId = _authService.GetCurrentSellerId();
                    if (!sellerId.HasValue || _authService.GetCurrentRole() != UserRole.Seller)
                    {
                        LogWarning("Unauthorized access to seller order items");
                        return Unauthorized("Only sellers can access this page.");
                    }
                    ViewBag.Games = await _gameService.GetAllGamesAsync();

                    
                    var orderItems = await _orderItemRepository.GetBySellerIdAsync(sellerId.Value, UserRole.Seller);
                    var orderItemDTOs = orderItems.Select(oi => new OrderItemDTO
                    {
                        Id = oi.Id,
                        OrderId = oi.OrderId,
                        GameId = oi.GameId,
                        SellerId = oi.SellerId,
                        Key = oi.Key
                    }).ToList();

                    
                    LogInformation("Retrieved {Count} order items and {GameCount} games for seller ID: {SellerId}", orderItemDTOs.Count, ViewBag.Games?.Count ?? 0, sellerId);
                    return View(orderItemDTOs);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving order items or games for seller");
                    ViewBag.ErrorMessage = "Произошла ошибка при загрузке элементов заказа или списка игр.";
                    return View(new List<OrderItemDTO>());
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateKey(Guid gameId, string key)
        {
            using (PushLogContext())
            {
                try
                {
                    var sellerId = _authService.GetCurrentSellerId();
                    if (!sellerId.HasValue || _authService.GetCurrentRole() != UserRole.Seller)
                    {
                        LogWarning("Unauthorized attempt to create key");
                        return Unauthorized("Only sellers can create keys.");
                    }

                    await _sellerService.CreateKeyAsync(gameId, key);
                    LogInformation("Key created for game ID: {GameId} by seller ID: {SellerId}", gameId, sellerId);
                    return RedirectToAction("OrderItems");
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error creating key for game ID: {GameId}", gameId);
                    TempData["ErrorMessage"] = $"Произошла ошибка при создании ключа: {ex.Message}";
                    return RedirectToAction("OrderItems");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetOrderItemKey(Guid orderItemId, string key)
        {
            using (PushLogContext())
            {
                try
                {
                    var sellerId = _authService.GetCurrentSellerId();
                    if (!sellerId.HasValue || _authService.GetCurrentRole() != UserRole.Seller)
                    {
                        LogWarning("Unauthorized attempt to set order item key");
                        return Unauthorized("Only sellers can set keys.");
                    }

                    var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, UserRole.Seller, _authService.GetCurrentUserId());
                    if (orderItem.SellerId != sellerId.Value)
                    {
                        LogWarning("Seller {SellerId} attempted to set key for unauthorized order item {OrderItemId}", sellerId, orderItemId);
                        return Unauthorized("You can only set keys for your own order items.");
                    }

                    orderItem.SetKey(key);
                    await _orderItemRepository.UpdateAsync(orderItem, UserRole.Seller);
                    LogInformation("Key set for order item ID: {OrderItemId} by seller ID: {SellerId}", orderItemId, sellerId);
                    return RedirectToAction("OrderItems");
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error setting key for order item ID: {OrderItemId}", orderItemId);
                    TempData["ErrorMessage"] = $"Произошла ошибка при установке ключа: {ex.Message}";
                    return RedirectToAction("OrderItems");
                }
            }
        }
    }
}