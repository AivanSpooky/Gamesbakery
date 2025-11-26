using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Controllers;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.WebGUI.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class GiftController : BaseController
    {
        private readonly IGiftService giftService;
        private readonly IUserService userService;

        public GiftController(IGiftService giftService, IUserService userService, IConfiguration configuration)
            : base(Log.ForContext<GiftController>(), configuration)
        {
            this.giftService = giftService;
            this.userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = this.GetCurrentUserId();
                if (userId == null)
                    return this.RedirectToAction("Login", "Account");
                var role = this.GetCurrentRole();
                var curUserId = this.GetCurrentUserId();
                var sentGifts = await this.giftService.GetGiftsBySenderAsync(userId.Value, curUserId, role);
                var receivedGifts = await this.giftService.GetGiftsByRecipientAsync(userId.Value, curUserId, role);
                var viewModel = new GiftIndexViewModel
                {
                    SentGifts = sentGifts.Select(g => new GiftResponseDTO
                    {
                        GiftId = g.GiftId,
                        SenderId = g.SenderId,
                        RecipientId = g.RecipientId,
                        OrderItemId = g.OrderItemId,
                        GiftDate = g.GiftDate,
                        GameTitle = g.GameTitle,
                    }),
                    ReceivedGifts = receivedGifts.Select(g => new GiftResponseDTO
                    {
                        GiftId = g.GiftId,
                        SenderId = g.SenderId,
                        RecipientId = g.RecipientId,
                        OrderItemId = g.OrderItemId,
                        GiftDate = g.GiftDate,
                        GameTitle = g.GameTitle,
                    }),
                };
                return this.View(viewModel);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading gifts");
                this.TempData["ErrorMessage"] = "Ошибка загрузки подарков.";
                return this.View(new GiftIndexViewModel());
            }
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create()
        {
            var userId = this.GetCurrentUserId();
            var role = this.GetCurrentRole();
            try
            {
                if (userId == null)
                    return this.RedirectToAction("Login", "Account");
                var users = await this.userService.GetAllUsersExceptAsync(userId.Value, role);
                var orderItems = await this.giftService.GetAvailableOrderItemsAsync(userId.Value, role);
                this.ViewBag.RecipientUsers = new SelectList(
                    users.Select(u => new UserListDTO
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                    }), "Id",
                    "Username");
                this.ViewBag.OrderItems = new SelectList(
                    orderItems.Select(oi => new OrderItemResponseDTO
                    {
                        Id = oi.Id,
                        GameId = oi.GameId,
                        GameTitle = oi.GameTitle,
                        SellerId = oi.SellerId,
                        SellerName = oi.SellerName,
                    }),
                    "Id",
                    "GameTitle");
                return this.View(new GiftCreateViewModel());
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading create gift form");
                this.TempData["ErrorMessage"] = "Ошибка загрузки формы.";
                return this.View(new GiftCreateViewModel());
            }
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(GiftCreateViewModel model)
        {
            var userId = this.GetCurrentUserId();
            var role = this.GetCurrentRole();
            try
            {
                if (userId == null)
                    return this.RedirectToAction("Login", "Account");
                if (!this.ModelState.IsValid)
                {
                    var users = await this.userService.GetAllUsersExceptAsync(userId.Value, role);
                    var orderItems = await this.giftService.GetAvailableOrderItemsAsync(userId.Value, role);
                    this.ViewBag.RecipientUsers = new SelectList(
                        users.Select(u => new UserListDTO
                        {
                            Id = u.Id,
                            Username = u.Username,
                            Email = u.Email,
                        }), "Id",
                        "Username");
                    this.ViewBag.OrderItems = new SelectList(
                        orderItems.Select(oi => new OrderItemResponseDTO
                        {
                            Id = oi.Id,
                            GameId = oi.GameId,
                            GameTitle = oi.GameTitle,
                            SellerId = oi.SellerId,
                            SellerName = oi.SellerName,
                        }), "Id",
                        "GameTitle");
                    return this.View(model);
                }

                var gift = await this.giftService.CreateGiftAsync(userId.Value, model.RecipientId, model.OrderItemId, userId, role);
                this.TempData["SuccessMessage"] = "Подарок успешно отправлен!";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error creating gift");
                this.TempData["ErrorMessage"] = $"Ошибка отправки подарка: {ex.Message}";
                var users = await this.userService.GetAllUsersExceptAsync(userId.Value, role);
                var orderItems = await this.giftService.GetAvailableOrderItemsAsync(userId.Value, role);
                this.ViewBag.RecipientUsers = new SelectList(
                    users.Select(u => new UserListDTO
                    {
                        Id = u.Id,
                        Username = u.Username,
                        Email = u.Email,
                    }), "Id",
                    "Username");
                this.ViewBag.OrderItems = new SelectList(
                    orderItems.Select(oi => new OrderItemResponseDTO
                    {
                        Id = oi.Id,
                        GameId = oi.GameId,
                        GameTitle = oi.GameTitle,
                        SellerId = oi.SellerId,
                        SellerName = oi.SellerName,
                    }), "Id",
                    "GameTitle");
                return this.View(model);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var userId = this.GetCurrentUserId();
                if (userId == null)
                    return this.RedirectToAction("Login", "Account");
                var role = this.GetCurrentRole();
                var gift = await this.giftService.GetGiftByIdAsync(id, userId, role);
                if (gift == null || (gift.SenderId != userId && gift.RecipientId != userId && role != UserRole.Admin))
                {
                    this.TempData["ErrorMessage"] = "Подарок не найден или доступ запрещен.";
                    return this.RedirectToAction("Index");
                }

                var giftResponse = new GiftResponseDTO
                {
                    GiftId = gift.GiftId,
                    SenderId = gift.SenderId,
                    RecipientId = gift.RecipientId,
                    OrderItemId = gift.OrderItemId,
                    GiftDate = gift.GiftDate,
                    GameTitle = gift.GameTitle,
                };
                return this.View(giftResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error retrieving gift {GiftId}", id);
                this.TempData["ErrorMessage"] = $"Ошибка загрузки подарка: {ex.Message}";
                return this.RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var role = this.GetCurrentRole();
                await this.giftService.DeleteGiftAsync(id, role);
                this.TempData["SuccessMessage"] = "Подарок удален успешно.";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error deleting gift {GiftId}", id);
                this.TempData["ErrorMessage"] = $"Ошибка удаления подарка: {ex.Message}";
                return this.RedirectToAction("Index");
            }
        }
    }
}
