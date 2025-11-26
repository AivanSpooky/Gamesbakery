using System;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Core.DTOs.UserDTO;
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
        private readonly IUserService userService;
        private readonly IAuthenticationService authService;
        private readonly IDatabaseConnectionChecker dbChecker;

        public UserController(
            IUserService userService,
            IAuthenticationService authService,
            IDatabaseConnectionChecker dbChecker,
            IConfiguration configuration)
            : base(Log.ForContext<UserController>(), configuration)
        {
            this.userService = userService;
            this.authService = authService;
            this.dbChecker = dbChecker;
        }

        [HttpGet]
        public IActionResult Register()
        {
            using (this.PushLogContext())
            {
                this.LogInformation("User accessed registration page");
                return this.View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            using (this.PushLogContext())
            {
                try
                {
                    this.LogInformation(
                        "User attempted to register with parameters: Username={Username}, Email={Email}, Country={Country}",
                        model.Username,
                        model.Email,
                        model.Country);
                    if (!await this.dbChecker.CanConnectAsync())
                    {
                        this.LogError(new InvalidOperationException("Database unavailable"), "Database connection failed during registration");
                        this.ModelState.AddModelError(string.Empty, "База данных недоступна.");
                        return this.View(model);
                    }

                    if (this.ModelState.IsValid)
                    {
                        var user = await this.userService.RegisterUserAsync(
                            model.Username,
                            model.Email,
                            model.Password,
                            model.Country);
                        this.LogInformation("Successfully registered user Username={Username}", model.Username);
                        return this.RedirectToAction("Login", "Account");
                    }

                    this.LogWarning("Invalid model state for user registration");
                    return this.View(model);
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "Error registering user Username={Username}", model.Username);
                    this.ModelState.AddModelError(string.Empty, $"Ошибка при регистрации: {ex.Message}");
                    return this.View(model);
                }
            }
        }

        public async Task<IActionResult> Profile()
        {
            var userId = this.GetCurrentUserId();
            var role = this.GetCurrentRole();
            try
            {
                if (userId == null)
                    return this.RedirectToAction("Login", "Account");
                var user = await this.userService.GetUserByIdAsync(userId.Value, userId, role);
                var userResponse = new UserResponseDTO
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    RegistrationDate = user.RegistrationDate,
                    Country = user.Country,
                    Balance = user.Balance,
                    TotalSpent = user.TotalSpent,
                };
                return this.View(userResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error retrieving user profile");
                this.ViewBag.ErrorMessage = $"Ошибка: {ex.Message}";
                return this.View();
            }
        }

        [HttpGet]
        public IActionResult UpdateBalance()
        {
            using (this.PushLogContext())
            {
                try
                {
                    this.LogInformation("User accessed balance update page");
                    if (this.GetCurrentUserId() == null)
                    {
                        this.LogWarning("Unauthorized access to balance update");
                        return this.Unauthorized("Требуется авторизация.");
                    }

                    return this.View();
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "Error accessing balance update page");
                    this.ViewBag.ErrorMessage = $"Ошибка загрузки формы: {ex.Message}";
                    return this.View();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBalance(decimal newBalance)
        {
            using (this.PushLogContext())
            {
                var role = this.GetCurrentRole();
                var userId = this.GetCurrentUserId();
                try
                {
                    this.LogInformation("User attempted to update balance with NewBalance={NewBalance}", newBalance);
                    if (userId == null)
                    {
                        this.LogWarning("Unauthorized attempt to update balance");
                        return this.Unauthorized("Требуется авторизация.");
                    }

                    if (!await this.dbChecker.CanConnectAsync())
                    {
                        this.LogError(new InvalidOperationException("Database unavailable"), "Database connection failed during balance update");
                        this.ModelState.AddModelError(string.Empty, "База данных недоступна.");
                        return this.View();
                    }

                    if (newBalance < 0)
                    {
                        this.LogWarning("Invalid balance update: NewBalance={NewBalance} is negative", newBalance);
                        this.ModelState.AddModelError(string.Empty, "Баланс не может быть отрицательным.");
                        return this.View();
                    }

                    var user = await this.userService.UpdateBalanceAsync(userId.Value, newBalance, userId, role);
                    this.LogInformation("Successfully updated balance for UserId={UserId} to {NewBalance}", userId, newBalance);
                    return this.RedirectToAction(nameof(this.Profile));
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "Error updating balance for UserId={UserId}", this.GetCurrentUserId());
                    this.ModelState.AddModelError(string.Empty, $"Ошибка при обновлении баланса: {ex.Message}");
                    return this.View();
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Block(Guid userId)
        {
            using (this.PushLogContext())
            {
                var role = this.GetCurrentRole();
                try
                {
                    this.LogInformation("User attempted to block user with UserId={UserId}", userId);
                    if (role != UserRole.Admin)
                    {
                        this.LogWarning("Unauthorized attempt to block user by Role={Role}", role);
                        return this.Unauthorized("Только администраторы могут блокировать пользователей.");
                    }

                    if (!await this.dbChecker.CanConnectAsync())
                    {
                        this.LogError(new InvalidOperationException("Database unavailable"), "Database connection failed during user block");
                        return this.StatusCode(503, "База данных недоступна.");
                    }

                    var user = await this.userService.BlockUserAsync(userId, role);
                    this.LogInformation("Successfully blocked user UserId={UserId}", userId);
                    return this.RedirectToAction(nameof(this.Profile));
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "Error blocking user UserId={UserId}", userId);
                    return this.StatusCode(500, $"Ошибка при блокировке пользователя: {ex.Message}");
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Unblock(Guid userId)
        {
            using (this.PushLogContext())
            {
                var role = this.GetCurrentRole();
                try
                {
                    this.LogInformation("User attempted to unblock user with UserId={UserId}", userId);
                    if (role != UserRole.Admin)
                    {
                        this.LogWarning("Unauthorized attempt to unblock user by Role={Role}", role);
                        return this.Unauthorized("Только администраторы могут разблокировать пользователей.");
                    }

                    if (!await this.dbChecker.CanConnectAsync())
                    {
                        this.LogError(new InvalidOperationException("Database unavailable"), "Database connection failed during user unblock");
                        return this.StatusCode(503, "База данных недоступна.");
                    }

                    var user = await this.userService.UnblockUserAsync(userId, role);
                    this.LogInformation("Successfully unblocked user UserId={UserId}", userId);
                    return this.RedirectToAction(nameof(this.Profile));
                }
                catch (Exception ex)
                {
                    this.LogError(ex, "Error unblocking user UserId={UserId}", userId);
                    return this.StatusCode(500, $"Ошибка при разблокировке пользователя: {ex.Message}");
                }
            }
        }
    }
}
