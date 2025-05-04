using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Gamesbakery.Infrastructure;
using Serilog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gamesbakery.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IGameService _gameService;
        private readonly IAuthenticationService _authService;
        private readonly IDatabaseConnectionChecker _dbChecker;
        private readonly IUserService _userService;

        public OrderController(
            IOrderService orderService,
            IGameService gameService,
            IAuthenticationService authService,
            IDatabaseConnectionChecker dbChecker,
            IUserService userService,
            IConfiguration configuration)
            : base(Log.ForContext<OrderController>(), configuration)
        {
            _orderService = orderService;
            _gameService = gameService;
            _authService = authService;
            _dbChecker = dbChecker;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed order list");
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View(new List<OrderListDTO>());
                    }

                    var userId = _authService.GetCurrentUserId();
                    if (userId == null && _authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized access to orders by Role={Role}", HttpContext.Session.GetString("Role"));
                        return Unauthorized("Требуется авторизация.");
                    }

                    List<OrderListDTO> orders;
                    if (_authService.GetCurrentRole() == UserRole.Admin)
                    {
                        orders = await _orderService.GetOrdersByUserIdAsync(Guid.Empty);
                        LogInformation("Admin retrieved all orders");
                    }
                    else
                    {
                        orders = await _orderService.GetOrdersByUserIdAsync(userId.Value);
                        LogInformation("User retrieved orders for UserId={UserId}", userId);
                    }

                    return View(orders);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving orders");
                    ViewBag.ErrorMessage = $"Ошибка при загрузке заказов: {ex.Message}";
                    return View(new List<OrderListDTO>());
                }
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed order details with Id={Id}", id);
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View();
                    }

                    var order = await _orderService.GetOrderByIdAsync(id);
                    LogInformation("Successfully retrieved order details for Id={Id}", id);
                    return View(order);
                }
                catch (KeyNotFoundException ex)
                {
                    LogWarning("Order not found for Id={Id}", id);
                    return NotFound();
                }
                catch (UnauthorizedAccessException ex)
                {
                    LogWarning("Unauthorized access to order Id={Id}", id);
                    return Unauthorized(ex.Message);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving order details for Id={Id}", id);
                    throw;
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed order creation page");
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized access to order creation");
                        return Unauthorized("Требуется авторизация.");
                    }

                    ViewBag.Games = await _gameService.GetAllGamesAsync();
                    return View();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing order creation page");
                    throw;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(List<Guid> gameIds)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to create order with GameIds={GameIds}", string.Join(",", gameIds));
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized attempt to create order");
                        return Unauthorized("Требуется авторизация.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ModelState.AddModelError("", "База данных недоступна.");
                        ViewBag.Games = await _gameService.GetAllGamesAsync();
                        return View();
                    }

                    if (gameIds == null || !gameIds.Any())
                    {
                        LogWarning("No games selected for order");
                        ModelState.AddModelError("", "Выберите хотя бы одну игру.");
                        ViewBag.Games = await _gameService.GetAllGamesAsync();
                        return View();
                    }

                    var userId = _authService.GetCurrentUserId().Value;
                    var user = await _userService.GetUserByIdAsync(userId);
                    var games = await _gameService.GetAllGamesAsync();
                    var selectedGames = games.Where(g => gameIds.Contains(g.Id) && g.IsForSale).ToList();

                    if (!selectedGames.Any())
                    {
                        LogWarning("No valid games selected for order");
                        ModelState.AddModelError("", "Выбранные игры недоступны для покупки.");
                        ViewBag.Games = games;
                        return View();
                    }

                    var totalCost = selectedGames.Sum(g => g.Price);
                    if (user.Balance < totalCost)
                    {
                        LogWarning("Insufficient balance for order. UserId={UserId}, Balance={Balance}, TotalCost={TotalCost}", userId, user.Balance, totalCost);
                        ModelState.AddModelError("", $"Недостаточно средств. Ваш баланс: {user.Balance:C}, стоимость заказа: {totalCost:C}.");
                        ViewBag.Games = games;
                        return View();
                    }

                    var order = await _orderService.CreateOrderAsync(userId, gameIds);
                    await _userService.UpdateBalanceAsync(userId, user.Balance - totalCost);
                    LogInformation("Successfully created order Id={OrderId} for UserId={UserId}, TotalCost={TotalCost}", order.Id, userId, totalCost);
                    return RedirectToAction(nameof(Details), new { id = order.Id });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error creating order");
                    ModelState.AddModelError("", $"Ошибка при создании заказа: {ex.Message}");
                    ViewBag.Games = await _gameService.GetAllGamesAsync();
                    return View();
                }
            }
        }
    }
}