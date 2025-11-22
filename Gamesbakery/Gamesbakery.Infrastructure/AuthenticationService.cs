using System;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.BusinessLogic.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserRepository _userRepository;

        public AuthenticationService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor, IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(UserRole Role, Guid? UserId, Guid? SellerId)> AuthenticateAsync(string username, string password)
        {
            try
            {
                Log.Information("Authenticating username: {Username}", username);

                // Check AdminUser
                if (username == _configuration["AdminCredentials:Username"])
                {
                    var adminPassword = _configuration["AdminCredentials:Password"];
                    if (password == adminPassword)
                    {
                        Log.Information("AdminUser authenticated successfully");
                        return (UserRole.Admin, null, null);
                    }
                }

                // Check GuestUser
                if (username == _configuration["GuestCredentials:Username"])
                {
                    var guestPassword = _configuration["GuestCredentials:Password"];
                    if (password == guestPassword)
                    {
                        Log.Information("GuestUser authenticated successfully");
                        return (UserRole.Guest, null, null);
                    }
                }

                // Database authentication using admin connection
                var adminConnectionString = _configuration.GetConnectionString("AdminConnection");
                using var connection = new SqlConnection(adminConnectionString);
                await connection.OpenAsync();

                // Check Users table
                using (var command = new SqlCommand("SELECT UserID FROM Users WHERE Name = @username AND Password = @password", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password); // TODO: Use hashing in production
                    var userId = await command.ExecuteScalarAsync();
                    if (userId != null)
                    {
                        Log.Information("User {Username} authenticated successfully", username);
                        return (UserRole.User, (Guid)userId, null);
                    }
                }

                // Check Sellers table
                using (var command = new SqlCommand("SELECT SellerID FROM Sellers WHERE Name = @username AND Password = @password", connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);
                    var sellerId = await command.ExecuteScalarAsync();
                    if (sellerId != null)
                    {
                        Log.Information("Seller {Username} authenticated successfully", username);
                        return (UserRole.Seller, null, (Guid)sellerId);
                    }
                }

                Log.Warning("Authentication failed for {Username}", username);
                return (UserRole.Guest, null, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Authentication error for {Username}", username);
                return (UserRole.Guest, null, null);
            }
        }

        public Guid? GetCurrentUserId()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            return Guid.TryParse(userIdClaim, out var id) && id != Guid.Empty ? id : null;
        }

        public Guid? GetCurrentSellerId()
        {
            var sellerIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("SellerId")?.Value;
            return Guid.TryParse(sellerIdClaim, out var id) && id != Guid.Empty ? id : null;
        }

        public UserRole GetCurrentRole()
        {
            var roleClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("role")?.Value;
            return Enum.TryParse<UserRole>(roleClaim, true, out var role) ? role : UserRole.Guest;
        }

        public async Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(country))
            {
                throw new ArgumentException("All registration fields must be provided.");
            }
            var userDto = new UserProfileDTO
            {
                Id = Guid.NewGuid(),
                Username = username,
                Email = email,
                RegistrationDate = DateTime.UtcNow,
                Country = country,
                IsBlocked = false,
                Balance = 0m,
                TotalSpent = 0m
            };
            return await _userRepository.AddAsync(userDto, UserRole.User);
        }
    }
}