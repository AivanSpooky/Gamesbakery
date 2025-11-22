using System;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Infrastructure;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IAuthenticationService _authService;
        private readonly IDatabaseConnectionChecker _dbChecker;

        public UserController(
            IUserService userService,
            IAuthenticationService authService,
            IDatabaseConnectionChecker dbChecker,
            IConfiguration configuration)
            : base(Log.ForContext<UserController>(), configuration)
        {
            _userService = userService;
            _authService = authService;
            _dbChecker = dbChecker;
        }

        [HttpGet]
        public IActionResult Register()
        {
            using (PushLogContext())
            {
                LogInformation("User accessed registration page");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to register with parameters: Username={Username}, Email={Email}, Country={Country}",
                        model.Username, model.Email, model.Country);
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError(new InvalidOperationException("Database unavailable"), "Database connection failed during registration");
                        ModelState.AddModelError("", "База данных недоступна.");
                        return View(model);
                    }
                    if (ModelState.IsValid)
                    {
                        var user = await _userService.RegisterUserAsync(
                            model.Username,
                            model.Email,
                            model.Password,
                            model.Country);
                        LogInformation("Successfully registered user Username={Username}", model.Username);
                        return RedirectToAction("Login", "Account");
                    }
                    LogWarning("Invalid model state for user registration");
                    return View(model);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error registering user Username={Username}", model.Username);
                    ModelState.AddModelError("", $"Ошибка при регистрации: {ex.Message}");
                    return View(model);
                }
            }
        }

        public async Task<IActionResult> Profile()
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentRole();
            try
            {
                if (userId == null)
                    return RedirectToAction("Login", "Account");
                var user = await _userService.GetUserByIdAsync(userId.Value, userId, role);
                var userResponse = new UserResponseDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RegistrationDate = user.RegistrationDate,
                    Country = user.Country,
                    Balance = user.Balance,
                    TotalSpent = user.TotalSpent
                };
                return View(userResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error retrieving user profile");
                ViewBag.ErrorMessage = $"Ошибка: {ex.Message}";
                return View();
            }
        }

        [HttpGet]
        public IActionResult UpdateBalance()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed balance update page");
                    if (GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized access to balance update");
                        return Unauthorized("Требуется авторизация.");
                    }
                    return View();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing balance update page");
                    ViewBag.ErrorMessage = $"Ошибка загрузки формы: {ex.Message}";
                    return View();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBalance(decimal newBalance)
        {
            using (PushLogContext())
            {
                var role = GetCurrentRole();
                var userId = GetCurrentUserId();
                try
                {
                    LogInformation("User attempted to update balance with NewBalance={NewBalance}", newBalance);
                    if (userId == null)
                    {
                        LogWarning("Unauthorized attempt to update balance");
                        return Unauthorized("Требуется авторизация.");
                    }
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError(new InvalidOperationException("Database unavailable"), "Database connection failed during balance update");
                        ModelState.AddModelError("", "База данных недоступна.");
                        return View();
                    }
                    if (newBalance < 0)
                    {
                        LogWarning("Invalid balance update: NewBalance={NewBalance} is negative", newBalance);
                        ModelState.AddModelError("", "Баланс не может быть отрицательным.");
                        return View();
                    }
                    var user = await _userService.UpdateBalanceAsync(userId.Value, newBalance, userId, role);
                    LogInformation("Successfully updated balance for UserId={UserId} to {NewBalance}", userId, newBalance);
                    return RedirectToAction(nameof(Profile));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error updating balance for UserId={UserId}", GetCurrentUserId());
                    ModelState.AddModelError("", $"Ошибка при обновлении баланса: {ex.Message}");
                    return View();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Block(Guid userId)
        {
            using (PushLogContext())
            {
                var role = GetCurrentRole();
                try
                {
                    LogInformation("User attempted to block user with UserId={UserId}", userId);
                    if (role != UserRole.Admin)
                    {
                        LogWarning("Unauthorized attempt to block user by Role={Role}", role);
                        return Unauthorized("Только администраторы могут блокировать пользователей.");
                    }
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError(new InvalidOperationException("Database unavailable"), "Database connection failed during user block");
                        return StatusCode(503, "База данных недоступна.");
                    }
                    var user = await _userService.BlockUserAsync(userId, role);
                    LogInformation("Successfully blocked user UserId={UserId}", userId);
                    return RedirectToAction(nameof(Profile));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error blocking user UserId={UserId}", userId);
                    return StatusCode(500, $"Ошибка при блокировке пользователя: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(Guid userId)
        {
            using (PushLogContext())
            {
                var role = GetCurrentRole();
                try
                {
                    LogInformation("User attempted to unblock user with UserId={UserId}", userId);
                    if (role != UserRole.Admin)
                    {
                        LogWarning("Unauthorized attempt to unblock user by Role={Role}", role);
                        return Unauthorized("Только администраторы могут разблокировать пользователей.");
                    }
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError(new InvalidOperationException("Database unavailable"), "Database connection failed during user unblock");
                        return StatusCode(503, "База данных недоступна.");
                    }
                    var user = await _userService.UnblockUserAsync(userId, role);
                    LogInformation("Successfully unblocked user UserId={UserId}", userId);
                    return RedirectToAction(nameof(Profile));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error unblocking user UserId={UserId}", userId);
                    return StatusCode(500, $"Ошибка при разблокировке пользователя: {ex.Message}");
                }
            }
        }
    }
}