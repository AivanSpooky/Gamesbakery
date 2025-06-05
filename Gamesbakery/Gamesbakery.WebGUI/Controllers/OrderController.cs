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
using Gamesbakery.Core.Repositories;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.Core.DTOs;

namespace Gamesbakery.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        private readonly IGameService _gameService;
        private readonly IAuthenticationService _authService;
        private readonly IDatabaseConnectionChecker _dbChecker;
        private readonly IUserService _userService;
        private readonly ICartService _cartService;
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderController(
        IOrderService orderService,
        IGameService gameService,
        IAuthenticationService authService,
        IDatabaseConnectionChecker dbChecker,
        IUserService userService,
        ICartService cartService,
        IOrderItemRepository orderItemRepository,
        IConfiguration configuration)
        : base(Log.ForContext<OrderController>(), configuration)
        {
            _orderService = orderService;
            _gameService = gameService;
            _authService = authService;
            _dbChecker = dbChecker;
            _userService = userService;
            _cartService = cartService;
            _orderItemRepository = orderItemRepository;
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
                    var role = _authService.GetCurrentRole();
                    LogInformation("Current UserId: {UserId}, Role: {Role}", userId, role);

                    if (userId == null && role != UserRole.Admin)
                    {
                        LogWarning("Unauthorized access to orders by Role={Role}", HttpContext.Session.GetString("Role"));
                        return Unauthorized("Требуется авторизация.");
                    }

                    List<OrderListDTO> orders;
                    if (role == UserRole.Admin)
                    {
                        orders = await _orderService.GetOrdersByUserIdAsync(Guid.Empty);
                        LogInformation("Admin retrieved {Count} orders", orders.Count);
                    }
                    else
                    {
                        if (userId == null)
                        {
                            LogError("UserId is null for non-admin role");
                            ViewBag.ErrorMessage = "Ошибка авторизации: пользователь не найден.";
                            return View(new List<OrderListDTO>());
                        }
                        orders = await _orderService.GetOrdersByUserIdAsync(userId.Value);
                        LogInformation("User retrieved {Count} orders for UserId={UserId}", orders.Count, userId);
                    }

                    if (!orders.Any())
                    {
                        LogInformation("No orders found for UserId={UserId}", userId);
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
                    var userId = _authService.GetCurrentUserId();
                    var role = _authService.GetCurrentRole();
                    LogInformation("User accessed order details with Id={Id}, UserId={UserId}, Role={Role}", id, userId, role);

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View();
                    }

                    var order = await _orderService.GetOrderByIdAsync(id);
                    ViewBag.OrderItems = await _orderItemRepository.GetByOrderIdAsync(id, UserRole.User);
                    LogInformation("Successfully retrieved order details for Id={Id}", id);
                    return View(order);
                }
                catch (KeyNotFoundException ex)
                {
                    LogWarning("Order not found for Id={Id}: {Message}", id, ex.Message);
                    ViewBag.ErrorMessage = "Заказ не найден или у вас нет доступа.";
                    return View();
                }
                catch (UnauthorizedAccessException ex)
                {
                    LogWarning("Unauthorized access to order Id={Id}: {Message}", id, ex.Message);
                    return Unauthorized(ex.Message);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving order details for Id={Id}", id);
                    ViewBag.ErrorMessage = $"Ошибка при загрузке деталей заказа: {ex.Message}";
                    return View();
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed cart");
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized access to cart");
                        return Unauthorized("Требуется авторизация.");
                    }

                    var cartItems = await _cartService.GetCartItemsAsync();
                    var cartDetails = new List<OrderItemGDTO>();
                    foreach (var item in cartItems)
                    {
                        try
                        {
                            var game = await _gameService.GetGameByIdAsync(item.GameId);
                            cartDetails.Add(new OrderItemGDTO
                            {
                                Id = item.Id,
                                GameId = item.GameId,
                                GameTitle = game.Title,
                                GamePrice = game.Price,
                                Key = item.Key
                            });
                        }
                        catch (KeyNotFoundException ex)
                        {
                            LogWarning("Game with ID {GameId} not found for cart item {OrderItemId}: {Message}", item.GameId, item.Id, ex.Message);
                            await _cartService.RemoveFromCartAsync(item.Id); // Remove invalid item
                        }
                    }

                    ViewBag.Total = await _cartService.GetCartTotalAsync();
                    LogInformation("Cart loaded with {Count} items", cartDetails.Count);

                    // Clear TempData to prevent error message persistence
                    if (TempData["ErrorMessage"] != null)
                    {
                        TempData.Keep("ErrorMessage"); // Keep for display
                        TempData.Remove("ErrorMessage"); // Clear after display
                    }

                    return View(cartDetails);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing cart");
                    ViewBag.ErrorMessage = $"Ошибка при загрузке корзины: {ex.Message}";
                    return View(new List<OrderItemGDTO>());
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(Guid orderItemId)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to add OrderItemId={OrderItemId} to cart", orderItemId);
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized attempt to add to cart");
                        return Unauthorized("Требуется авторизация.");
                    }

                    await _cartService.AddToCartAsync(orderItemId);
                    LogInformation("Successfully added OrderItemId={OrderItemId} to cart", orderItemId);
                    return RedirectToAction("Cart");
                }
                catch (KeyNotFoundException ex)
                {
                    LogWarning("Failed to add OrderItemId={OrderItemId} to cart: {Message}", orderItemId, ex.Message);
                    TempData["ErrorMessage"] = "Этот товар больше не доступен.";
                    return RedirectToAction("Index", "Game");
                }
                catch (InvalidOperationException ex)
                {
                    LogWarning("Failed to add OrderItemId={OrderItemId} to cart: {Message}", orderItemId, ex.Message);
                    TempData["ErrorMessage"] = "Этот товар уже добавлен в заказ.";
                    return RedirectToAction("Index", "Game");
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error adding OrderItemId={OrderItemId} to cart", orderItemId);
                    TempData["ErrorMessage"] = $"Ошибка при добавлении в корзину: {ex.Message}";
                    return RedirectToAction("Index", "Game");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(Guid orderItemId)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to remove OrderItemId={OrderItemId} from cart", orderItemId);
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized attempt to remove from cart");
                        return Unauthorized("Требуется авторизация.");
                    }

                    await _cartService.RemoveFromCartAsync(orderItemId);
                    LogInformation("Successfully removed OrderItemId={OrderItemId} from cart", orderItemId);
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error removing OrderItemId={OrderItemId} from cart", orderItemId);
                    TempData["ErrorMessage"] = $"Ошибка при удалении из корзины: {ex.Message}";
                    return RedirectToAction("Cart");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to clear cart");
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized attempt to clear cart");
                        return Unauthorized("Требуется авторизация.");
                    }

                    _cartService.ClearCart();
                    LogInformation("Successfully cleared cart");
                    TempData["SuccessMessage"] = "Корзина успешно очищена.";
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error clearing cart");
                    TempData["ErrorMessage"] = $"Ошибка при очистке корзины: {ex.Message}";
                    return RedirectToAction("Cart");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to checkout");
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized attempt to checkout");
                        return Unauthorized("Требуется авторизация.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        TempData["ErrorMessage"] = "База данных недоступна.";
                        return RedirectToAction("Cart");
                    }

                    var userId = _authService.GetCurrentUserId().Value;
                    var user = await _userService.GetUserByIdAsync(userId);
                    var cartItems = await _cartService.GetCartItemsAsync();

                    if (!cartItems.Any())
                    {
                        LogWarning("Cart is empty during checkout");
                        TempData["ErrorMessage"] = "Корзина пуста.";
                        return RedirectToAction("Cart");
                    }

                    var totalCost = await _cartService.GetCartTotalAsync();
                    if (user.Balance < totalCost)
                    {
                        LogWarning("Insufficient balance for checkout. UserId={UserId}, Balance={Balance}, TotalCost={TotalCost}", userId, user.Balance, totalCost);
                        TempData["ErrorMessage"] = $"Недостаточно средств. Ваш баланс: {user.Balance:C}, стоимость заказа: {totalCost:C}.";
                        return RedirectToAction("Cart");
                    }

                    var orderItemIds = cartItems.Select(i => i.Id).ToList();
                    var order = await _orderService.CreateOrderAsync(userId, orderItemIds);
                    _cartService.ClearCart(); // Clear cart only after successful order creation

                    LogInformation("Successfully created order Id={OrderId} for UserId={UserId}, TotalCost={TotalCost}", order.Id, userId, totalCost);
                    return RedirectToAction("Details", new { id = order.Id });
                }
                catch (KeyNotFoundException ex)
                {
                    LogWarning("Checkout failed: {Message}", ex.Message);
                    TempData["ErrorMessage"] = $"Ошибка при оформлении заказа: {ex.Message}";
                    return RedirectToAction("Cart");
                }
                catch (InvalidOperationException ex)
                {
                    LogWarning("Checkout failed: {Message}", ex.Message);
                    if (ex.Message.Contains("Insufficient balance"))
                        TempData["ErrorMessage"] = ex.Message;
                    else if (ex.Message.Contains("already part of an order"))
                        TempData["ErrorMessage"] = "Один или несколько товаров уже добавлены в другой заказ.";
                    else
                        TempData["ErrorMessage"] = $"Ошибка при оформлении заказа: {ex.Message}";
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error during checkout");
                    TempData["ErrorMessage"] = $"Ошибка при оформлении заказа: {ex.Message}";
                    return RedirectToAction("Cart");
                }
            }
        }
    }
}