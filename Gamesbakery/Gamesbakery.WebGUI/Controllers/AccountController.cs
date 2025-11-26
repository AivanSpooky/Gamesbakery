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
        private readonly IAuthenticationService _authService;
        private readonly IConfiguration _configuration;

        public AccountController(IAuthenticationService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View(new LoginViewModel());
        }

        public async Task<IActionResult> ApiLogin([FromBody] LoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { error = "Invalid input" });
            var (role, userId, sellerId) = await _authService.AuthenticateAsync(dto.Username, dto.Password);
            if (role == UserRole.Guest)
                return Unauthorized(new { error = "Неверное имя пользователя или пароль" });
            var token = GenerateJwtToken(dto.Username, role, userId, sellerId);
            Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddHours(8)
            });
            return Ok(new SingleResponse<object>
            {
                Item = new { token, role = role.ToString(), userId, sellerId },
                Message = "Login successful"
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);
            var result = await ApiLogin(new LoginDTO
            {
                Username = model.Username,
                Password = model.Password
            });
            if (result is OkObjectResult)
                return RedirectToAction("Index", "Home");
            ModelState.AddModelError("", "Неверное имя пользователя или пароль.");
            return View(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("JwtToken", new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            });
            return RedirectToAction("Index", "Home");
        }

        private string GenerateJwtToken(string username, UserRole role, Guid? userId, Guid? sellerId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role.ToString())
            };
            if (userId.HasValue && userId != Guid.Empty)
                claims.Add(new Claim("UserId", userId.Value.ToString()));
            if (sellerId.HasValue && sellerId != Guid.Empty)
                claims.Add(new Claim("SellerId", sellerId.Value.ToString()));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
