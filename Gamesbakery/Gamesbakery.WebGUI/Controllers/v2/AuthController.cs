using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Context;
using IAuthenticationService = Gamesbakery.Core.IAuthenticationService;

namespace Gamesbakery.WebGUI.Controllers.v2
{
    /// <summary>
    /// Controller for handling authentication-related operations such as login, logout, and registration.
    /// </summary>
    [ApiController]
    [Route("api/v2/auth")]
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

        /// <summary>
        /// Authenticates a user and returns a JWT token and user details.
        /// </summary>
        /// <param name="request">The login credentials.</param>
        /// <returns>A response containing the JWT token, user role, user ID, seller ID, and a success message.</returns>
        /// <response code="200">Returns the authentication details upon successful login.</response>
        /// <response code="400">If username or password is missing.</response>
        /// <response code="401">If the credentials are invalid.</response>
        /// <response code="500">If an internal server error occurs.</response>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<object>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                    return BadRequest(new { error = "Username and password required" });
                var (role, userId, sellerId) = await _authService.AuthenticateAsync(request.Username, request.Password);
                if (role == UserRole.Guest)
                    return Unauthorized(new { error = "Invalid credentials" });
                var token = GenerateJwtToken(request.Username, role, userId, sellerId);
                Response.Cookies.Append("JwtToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.Scheme == "https",
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddHours(8)
                });
                return Ok(new SingleResponse<object>
                {
                    Item = new
                    {
                        token,
                        role = role.ToString(),
                        userId,
                        sellerId
                    },
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Internal server error", details = ex.Message });
            }
        }

        /// <summary>
        /// Logs out the authenticated user and clears the JWT token cookie.
        /// </summary>
        /// <returns>A response indicating successful logout.</returns>
        /// <response code="200">Returns a success message upon successful logout.</response>
        /// <response code="500">If an internal server error occurs during logout.</response>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<object>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                Response.Cookies.Delete("JwtToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.Scheme == "https",
                    SameSite = SameSiteMode.Lax
                });
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Ok(new SingleResponse<object>
                {
                    Item = null,
                    Message = "Successfully logged out"
                });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Logout failed");
                return StatusCode(500, new { error = "Logout failed", details = ex.Message });
            }
        }

        /// <summary>
        /// Registers a new user and returns a JWT token and user details.
        /// </summary>
        /// <param name="dto">The user registration details.</param>
        /// <returns>A response containing the JWT token, user details, and a success message.</returns>
        /// <response code="200">Returns the user details and token upon successful registration.</response>
        /// <response code="400">If the registration data is invalid.</response>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SingleResponse<UserResponseDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                return Ok(new SingleResponse<UserResponseDTO>
                {
                    Item = new UserResponseDTO
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        RegistrationDate = user.RegistrationDate,
                        Country = user.Country,
                        Balance = user.Balance,
                        TotalSpent = user.TotalSpent
                    },
                    Message = "Registration successful"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
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
    }
}