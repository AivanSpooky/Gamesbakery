using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    [Authorize(Roles = "Admin,Seller")]
    public class OrderItemController : BaseController
    {
        private readonly IOrderItemService orderItemService;

        public OrderItemController(IOrderItemService orderItemService, IConfiguration configuration)
            : base(Log.ForContext<OrderItemController>(), configuration)
        {
            this.orderItemService = orderItemService;
        }

        public async Task<IActionResult> Index(int page = 1, int limit = 10, Guid? sellerId = null, Guid? gameId = null)
        {
            try
            {
                var role = this.GetCurrentRole();
                if (role == UserRole.Seller)
                    sellerId = this.GetCurrentSellerId();
                var filteredItems = await this.orderItemService.GetFilteredAsync(sellerId, gameId, this.GetCurrentSellerId(), role);
                var totalCount = filteredItems.Count;
                var paginatedItems = filteredItems.Skip((page - 1) * limit).Take(limit).Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    GameId = oi.GameId,
                    GameTitle = oi.GameTitle,
                    SellerId = oi.SellerId,
                    SellerName = oi.SellerName,
                }).ToList();
                this.ViewBag.TotalCount = totalCount;
                this.ViewBag.Page = page;
                this.ViewBag.Limit = limit;
                return this.View(paginatedItems);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading order items");
                this.ViewBag.ErrorMessage = $"Ошибка загрузки товаров: {ex.Message}";
                return this.View(new List<OrderItemResponseDTO>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return this.View(new OrderItemCreateDTO());
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(OrderItemCreateDTO dto)
        {
            try
            {
                if (!this.ModelState.IsValid)
                    return this.View(dto);
                var role = this.GetCurrentRole();
                var curSellerId = this.GetCurrentSellerId();
                await this.orderItemService.CreateAsync(dto, curSellerId, role);
                this.TempData["SuccessMessage"] = "Товар успешно создан.";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error creating order item");
                this.ModelState.AddModelError(string.Empty, $"Ошибка при создании товара: {ex.Message}");
                return this.View(dto);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var role = this.GetCurrentRole();
                var curUserId = this.GetCurrentUserId();
                var item = await this.orderItemService.GetByIdAsync(id, curUserId, role);
                var itemResponse = new OrderItemResponseDTO
                {
                    Id = item.Id,
                    GameId = item.GameId,
                    GameTitle = item.GameTitle,
                    SellerId = item.SellerId,
                    SellerName = item.SellerName,
                };
                return this.View(itemResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading order item details {Id}", id);
                this.ViewBag.ErrorMessage = $"Ошибка загрузки деталей: {ex.Message}";
                return this.View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var role = this.GetCurrentRole();
                var curUserId = this.GetCurrentUserId();
                var item = await this.orderItemService.GetByIdAsync(id, curUserId, role);
                if (item == null)
                    return this.NotFound();
                return this.View(new OrderItemUpdateDTO { Key = item.Key });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading order item for edit {Id}", id);
                this.ViewBag.ErrorMessage = $"Ошибка загрузки формы редактирования: {ex.Message}";
                return this.View();
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit(Guid id, OrderItemUpdateDTO dto)
        {
            try
            {
                if (!this.ModelState.IsValid)
                    return this.View(dto);
                var role = this.GetCurrentRole();
                var curSellerId = this.GetCurrentSellerId();
                await this.orderItemService.UpdateAsync(id, dto, curSellerId, role);
                this.TempData["SuccessMessage"] = "Товар успешно обновлен.";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error updating order item {Id}", id);
                this.ModelState.AddModelError(string.Empty, $"Ошибка при обновлении товара: {ex.Message}");
                return this.View(dto);
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
                await this.orderItemService.DeleteAsync(id, role);
                this.TempData["SuccessMessage"] = "Товар удален успешно.";
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error deleting order item {Id}", id);
                this.TempData["ErrorMessage"] = $"Ошибка удаления товара: {ex.Message}";
                return this.RedirectToAction("Index");
            }
        }
    }
}
