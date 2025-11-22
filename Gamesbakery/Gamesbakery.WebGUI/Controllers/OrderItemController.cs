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
        private readonly IOrderItemService _orderItemService;

        public OrderItemController(IOrderItemService orderItemService, IConfiguration configuration)
            : base(Log.ForContext<OrderItemController>(), configuration)
        {
            _orderItemService = orderItemService;
        }

        public async Task<IActionResult> Index(int page = 1, int limit = 10, Guid? sellerId = null, Guid? gameId = null)
        {
            try
            {
                var role = GetCurrentRole();
                if (role == UserRole.Seller)
                    sellerId = GetCurrentSellerId();
                var filteredItems = await _orderItemService.GetFilteredAsync(sellerId, gameId, GetCurrentSellerId(), role);
                var totalCount = filteredItems.Count;
                var paginatedItems = filteredItems.Skip((page - 1) * limit).Take(limit).Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    GameId = oi.GameId,
                    GameTitle = oi.GameTitle,
                    SellerId = oi.SellerId,
                    SellerName = oi.SellerName
                }).ToList();
                ViewBag.TotalCount = totalCount;
                ViewBag.Page = page;
                ViewBag.Limit = limit;
                return View(paginatedItems);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading order items");
                ViewBag.ErrorMessage = $"Ошибка загрузки товаров: {ex.Message}";
                return View(new List<OrderItemResponseDTO>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new OrderItemCreateDTO());
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(OrderItemCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(dto);
                var role = GetCurrentRole();
                var curSellerId = GetCurrentSellerId();
                await _orderItemService.CreateAsync(dto, curSellerId, role);
                TempData["SuccessMessage"] = "Товар успешно создан.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error creating order item");
                ModelState.AddModelError("", $"Ошибка при создании товара: {ex.Message}");
                return View(dto);
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var role = GetCurrentRole();
                var curUserId = GetCurrentUserId();
                var item = await _orderItemService.GetByIdAsync(id, curUserId, role);
                var itemResponse = new OrderItemResponseDTO
                {
                    Id = item.Id,
                    GameId = item.GameId,
                    GameTitle = item.GameTitle,
                    SellerId = item.SellerId,
                    SellerName = item.SellerName
                };
                return View(itemResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading order item details {Id}", id);
                ViewBag.ErrorMessage = $"Ошибка загрузки деталей: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            try
            {
                var role = GetCurrentRole();
                var curUserId = GetCurrentUserId();
                var item = await _orderItemService.GetByIdAsync(id, curUserId, role);
                if (item == null)
                    return NotFound();
                return View(new OrderItemUpdateDTO { Key = item.Key });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading order item for edit {Id}", id);
                ViewBag.ErrorMessage = $"Ошибка загрузки формы редактирования: {ex.Message}";
                return View();
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit(Guid id, OrderItemUpdateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(dto);
                var role = GetCurrentRole();
                var curSellerId = GetCurrentSellerId();
                await _orderItemService.UpdateAsync(id, dto, curSellerId, role);
                TempData["SuccessMessage"] = "Товар успешно обновлен.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error updating order item {Id}", id);
                ModelState.AddModelError("", $"Ошибка при обновлении товара: {ex.Message}");
                return View(dto);
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var role = GetCurrentRole();
                await _orderItemService.DeleteAsync(id, role);
                TempData["SuccessMessage"] = "Товар удален успешно.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error deleting order item {Id}", id);
                TempData["ErrorMessage"] = $"Ошибка удаления товара: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}