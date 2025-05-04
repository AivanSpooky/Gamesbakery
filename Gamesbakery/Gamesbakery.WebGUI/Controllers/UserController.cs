using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Gamesbakery.Infrastructure;
using Gamesbakery.WebGUI.Models;
using Serilog;
using System;
using System.Threading.Tasks;

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
                        LogError("Database unavailable");
                        ModelState.AddModelError("", "База данных недоступна.");
                        return View(model);
                    }

                    if (ModelState.IsValid)
                    {
                        var user = await _userService.RegisterUserAsync(
                            model.Username,
                            model.Email,
                            "UserPass123",
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
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed profile");
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View();
                    }

                    var userId = _authService.GetCurrentUserId();
                    if (userId == null)
                    {
                        LogWarning("Unauthorized access to profile");
                        return Unauthorized("Требуется авторизация.");
                    }

                    var user = await _userService.GetUserByIdAsync(userId.Value);
                    LogInformation("Successfully retrieved profile for UserId={UserId}", userId);
                    return View(user);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving profile");
                    ViewBag.ErrorMessage = $"Ошибка при загрузке профиля: {ex.Message}";
                    return View();
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateBalance()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed balance update page");
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized access to balance update");
                        return Unauthorized("Требуется авторизация.");
                    }
                    return View();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing balance update page");
                    throw;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBalance(decimal newBalance)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to update balance with NewBalance={NewBalance}", newBalance);
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized attempt to update balance");
                        return Unauthorized("Требуется авторизация.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ModelState.AddModelError("", "База данных недоступна.");
                        return View();
                    }

                    if (newBalance < 0)
                    {
                        LogWarning("Invalid balance update: NewBalance={NewBalance} is negative", newBalance);
                        ModelState.AddModelError("", "Баланс не может быть отрицательным.");
                        return View();
                    }

                    var userId = _authService.GetCurrentUserId().Value;
                    var user = await _userService.UpdateBalanceAsync(userId, newBalance);
                    LogInformation("Successfully updated balance for UserId={UserId} to {NewBalance}", userId, newBalance);
                    return RedirectToAction(nameof(Profile));
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error updating balance");
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
                try
                {
                    LogInformation("User attempted to block user with UserId={UserId}", userId);
                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized attempt to block user by Role={Role}", HttpContext.Session.GetString("Role"));
                        return Unauthorized("Только администраторы могут блокировать пользователей.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        return StatusCode(503, "База данных недоступна.");
                    }

                    await _userService.BlockUserAsync(userId);
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
                try
                {
                    LogInformation("User attempted to unblock user with UserId={UserId}", userId);
                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized attempt to unblock user by Role={Role}", HttpContext.Session.GetString("Role"));
                        return Unauthorized("Только администраторы могут разблокировать пользователей.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        return StatusCode(503, "База данных недоступна.");
                    }

                    await _userService.UnblockUserAsync(userId);
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