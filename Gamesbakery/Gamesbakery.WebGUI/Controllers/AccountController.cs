using Gamesbakery.Core;
using Gamesbakery.WebGUI.Models;
using Gamesbakery.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Serilog;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using IAuthenticationService = Gamesbakery.Core.IAuthenticationService;
using Gamesbakery.Controllers;

namespace Gamesbakery.WebGUI.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IAuthenticationService _authService;

        public AccountController(IAuthenticationService authService, IConfiguration configuration)
            : base(Log.ForContext<AccountController>(), configuration)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            using (PushLogContext())
            {
                LogInformation("User accessed login page");
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted login with username: {Username}", model.Username);

                    if (!ModelState.IsValid)
                    {
                        LogWarning("Login failed for username: {Username}: Invalid model state", model.Username);
                        return View(model);
                    }

                    var (role, userId, sellerId) = await _authService.AuthenticateAsync(model.Username, model.Password);

                    if (role == UserRole.Guest && model.Username != "GuestUser")
                    {
                        ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
                        LogWarning("Login failed for username: {Username}: Invalid credentials", model.Username);
                        return View(model);
                    }

                    // Set session variables
                    HttpContext.Session.SetString("Username", model.Username);
                    HttpContext.Session.SetString("Role", role.ToString());
                    if (userId.HasValue)
                        HttpContext.Session.SetString("UserId", userId.Value.ToString());
                    if (sellerId.HasValue)
                        HttpContext.Session.SetString("SellerId", sellerId.Value.ToString());

                    // Create claims for non-Guest users
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.Username),
                        new Claim(ClaimTypes.Role, role.ToString())
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    LogInformation("Login successful for username: {Username}, role: {Role}, userId: {UserId}, sellerId: {SellerId}",
                        model.Username, role, userId, sellerId);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error during login for username: {Username}", model.Username);
                    ModelState.AddModelError("", $"Произошла ошибка при входе: {ex.Message}");
                    return View(model);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User logged out");
                    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    HttpContext.Session.Clear();
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error during logout");
                    throw;
                }
            }
        }
    }
}