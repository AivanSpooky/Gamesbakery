using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Gamesbakery.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly IAuthenticationService authService;
        private readonly IConfiguration configuration;

        public AccountController(IAuthenticationService authService, IConfiguration configuration)
        {
            this.authService = authService;
            this.configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (this.User.Identity.IsAuthenticated)
                return this.RedirectToAction("Index", "Home");
            return this.View(new LoginViewModel());
        }

        public async Task<IActionResult> ApiLogin([FromBody] LoginDTO dto)
        {
            if (!this.ModelState.IsValid)
                return this.BadRequest(new { error = "Invalid input" });
            var (role, userId, sellerId) = await this.authService.AuthenticateAsync(dto.Username, dto.Password);
            if (role == UserRole.Guest)
                return this.Unauthorized(new { error = "Неверное имя пользователя или пароль" });
            var token = this.GenerateJwtToken(dto.Username, role, userId, sellerId);
            this.Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = this.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(8),
            });
            return this.Ok(new SingleResponse<object>
            {
                Item = new { token, role = role.ToString(), userId, sellerId },
                Message = "Login successful",
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!this.ModelState.IsValid)
                return this.View(model);
            var result = await this.ApiLogin(new LoginDTO
            {
                Username = model.Username,
                Password = model.Password,
            });
            if (result is OkObjectResult)
                return this.RedirectToAction("Index", "Home");
            this.ModelState.AddModelError(string.Empty, "Неверное имя пользователя или пароль.");
            return this.View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Logout()
        {
            this.Response.Cookies.Delete("JwtToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = this.Request.IsHttps,
                SameSite = SameSiteMode.Lax,
            });
            return this.RedirectToAction("Index", "Home");
        }

        private string GenerateJwtToken(string username, UserRole role, Guid? userId, Guid? sellerId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role.ToString()),
            };
            if (userId.HasValue && userId != Guid.Empty)
                claims.Add(new Claim("UserId", userId.Value.ToString()));
            if (sellerId.HasValue && sellerId != Guid.Empty)
                claims.Add(new Claim("SellerId", sellerId.Value.ToString()));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: this.configuration["Jwt:Issuer"],
                audience: this.configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
