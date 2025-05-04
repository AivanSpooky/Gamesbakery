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

namespace Gamesbakery.Controllers
{
    public class SellerController : BaseController
    {
        private readonly ISellerService _sellerService;
        private readonly IOrderService _orderService;
        private readonly IAuthenticationService _authService;
        private readonly IDatabaseConnectionChecker _dbChecker;

        public SellerController(
            ISellerService sellerService,
            IOrderService orderService,
            IAuthenticationService authService,
            IDatabaseConnectionChecker dbChecker,
            IConfiguration configuration)
            : base(Log.ForContext<SellerController>(), configuration)
        {
            _sellerService = sellerService;
            _orderService = orderService;
            _authService = authService;
            _dbChecker = dbChecker;
        }

        [HttpGet]
        public IActionResult Register()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed seller registration page");
                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized access to seller registration by Role={Role}", HttpContext.Session.GetString("Role"));
                        return Unauthorized("Только администраторы могут регистрировать продавцов.");
                    }
                    return View();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing seller registration page");
                    throw;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(SellerRegisterViewModel model)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to register seller with SellerName={SellerName}", model.SellerName);
                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized attempt to register seller by Role={Role}", HttpContext.Session.GetString("Role"));
                        return Unauthorized("Только администраторы могут регистрировать продавцов.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ModelState.AddModelError("", "База данных недоступна.");
                        return View(model);
                    }

                    if (ModelState.IsValid)
                    {
                        await _sellerService.RegisterSellerAsync(model.SellerName, model.Password);
                        LogInformation("Successfully registered seller SellerName={SellerName}", model.SellerName);
                        return RedirectToAction("Index", "Home");
                    }

                    LogWarning("Invalid model state for seller registration");
                    return View(model);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error registering seller SellerName={SellerName}", model.SellerName);
                    ModelState.AddModelError("", $"Ошибка при регистрации продавца: {ex.Message}");
                    return View(model);
                }
            }
        }

        public async Task<IActionResult> Profile()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed seller profile");
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View();
                    }

                    var sellerId = _authService.GetCurrentSellerId();
                    if (sellerId == null)
                    {
                        LogWarning("Unauthorized access to seller profile");
                        return Unauthorized("Требуется авторизация продавца.");
                    }

                    var seller = await _sellerService.GetSellerByIdAsync(sellerId.Value);
                    LogInformation("Successfully retrieved seller profile for SellerId={SellerId}", sellerId);
                    return View(seller);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving seller profile");
                    ViewBag.ErrorMessage = $"Ошибка при загрузке профиля: {ex.Message}";
                    return View();
                }
            }
        }

        public async Task<IActionResult> OrderItems()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed seller order items");
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View(new List<OrderItemDTO>());
                    }

                    var sellerId = _authService.GetCurrentSellerId();
                    if (sellerId == null)
                    {
                        LogWarning("Unauthorized access to order items");
                        return Unauthorized("Требуется авторизация продавца.");
                    }

                    var orderItems = await _orderService.GetOrderItemsBySellerIdAsync(sellerId.Value);
                    LogInformation("Successfully retrieved order items for SellerId={SellerId}", sellerId);
                    return View(orderItems);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving order items");
                    ViewBag.ErrorMessage = $"Ошибка при загрузке элементов заказа: {ex.Message}";
                    return View(new List<OrderItemDTO>());
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
                    LogInformation("User attempted to set order item key with OrderItemId={OrderItemId}, Key={Key}", orderItemId, key);
                    if (_authService.GetCurrentSellerId() == null)
                    {
                        LogWarning("Unauthorized attempt to set order item key");
                        return Unauthorized("Требуется авторизация продавца.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        return StatusCode(503, "База данных недоступна.");
                    }

                    var sellerId = _authService.GetCurrentSellerId().Value;
                    await _orderService.SetOrderItemKeyAsync(orderItemId, key, sellerId);
                    LogInformation("Successfully set order item key for OrderItemId={OrderItemId} by SellerId={SellerId}", orderItemId, sellerId);
                    return RedirectToAction(nameof(OrderItems));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error setting order item key for OrderItemId={OrderItemId}", orderItemId);
                    return StatusCode(500, $"Ошибка при установке ключа: {ex.Message}");
                }
            }
        }
    }
}