using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.UserDTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Context;
using IAuthenticationService = Gamesbakery.Core.IAuthenticationService;

namespace Gamesbakery.WebGUI.Controllers.v1
{
    [ApiController]
    [Route("api/v1/auth")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    [IgnoreAntiforgeryToken]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        public AuthController(
            IAuthenticationService authService,
            IUserService userService,
            IConfiguration configuration)
        {
            _userService = userService;
            _authService = authService;
            _configuration = configuration;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    return BadRequest(new { error = "Username and password required" });
                var (role, userId, sellerId) = await _authService.AuthenticateAsync(request.Username, request.Password);
                if (role == UserRole.Guest)
                    return Unauthorized(new { error = "Invalid credentials" });
                // Создаем JWT токен
                var token = GenerateJwtToken(request.Username, role, userId, sellerId);
                // Устанавливаем cookie для WebGUI
                Response.Cookies.Append("JwtToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.Scheme == "https",
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddHours(8)
                });
                return Ok(new
                {
                    token = token,
                    role = role.ToString(),
                    userId = userId,
                    sellerId = sellerId,
                    message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }
        private string GenerateJwtToken(string username, UserRole role, Guid? userId, Guid? sellerId)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.Role, role.ToString())
            };
            if (userId.HasValue)
                claims.Add(new Claim("UserId", userId.Value.ToString()));
            if (sellerId.HasValue)
                claims.Add(new Claim("SellerId", sellerId.Value.ToString()));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var user = await _userService.RegisterUserAsync(dto.Username, dto.Email, dto.Password, dto.Country);
                var token = GenerateJwtToken(dto.Username, UserRole.User, user.Id, null);
                Response.Cookies.Append("JwtToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.Scheme == "https",
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddHours(8)
                });
                return Ok(new
                {
                    token,
                    user = new
                    {
                        id = user.Id,
                        username = user.Username,
                        email = user.Email
                    },
                    message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}