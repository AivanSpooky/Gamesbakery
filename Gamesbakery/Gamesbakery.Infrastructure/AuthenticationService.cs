using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.SqlClient;
using Serilog;

namespace Gamesbakery.BusinessLogic.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<(UserRole Role, Guid? UserId, Guid? SellerId)> AuthenticateAsync(string username, string password)
        {
            try
            {
                // Check AdminUser (no DB query)
                if (username == _configuration["AdminCredentials:Username"])
                {
                    var adminPassword = _configuration["AdminCredentials:Password"];
                    if (password == adminPassword)
                    {
                        Log.Information("AdminUser authenticated successfully");
                        return (UserRole.Admin, null, null);
                    }
                    Log.Warning("Invalid password for AdminUser");
                    return (UserRole.Guest, null, null);
                }

                // Check GuestUser (no DB query)
                if (username == _configuration["GuestCredentials:Username"])
                {
                    var guestPassword = _configuration["GuestCredentials:Password"];
                    if (password == guestPassword)
                    {
                        Log.Information("GuestUser authenticated successfully");
                        return (UserRole.Guest, null, null);
                    }
                    Log.Warning("Invalid password for GuestUser");
                    return (UserRole.Guest, null, null);
                }

                // Use admin connection for Users and Sellers
                var adminConnectionString = _configuration.GetConnectionString("AdminConnection");
                var options = new DbContextOptionsBuilder<GamesbakeryDbContext>()
                    .UseSqlServer(adminConnectionString)
                    .Options;
                Log.Information($"CONN STR: {adminConnectionString}");

                // Используем чистый SQL через ADO.NET вместо EF контекста
                using (var connection = new SqlConnection(adminConnectionString))
                {
                    await connection.OpenAsync();

                    // Check Users table
                    using (var command = new SqlCommand("SELECT UserID, Name, Password FROM Users WHERE Name = @username", connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var userId = reader.GetGuid(reader.GetOrdinal("UserID"));
                                var storedPassword = reader.GetString(reader.GetOrdinal("Password"));
                                if (VerifyPassword(password, storedPassword))
                                {
                                    Log.Information("User {Username} authenticated successfully", username);
                                    return (UserRole.User, userId, null);
                                }
                            }
                        }
                    }

                    // Check Sellers table
                    using (var command = new SqlCommand("SELECT SellerID, SellerName, Password FROM Sellers WHERE Name = @username", connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var sellerId = reader.GetGuid(reader.GetOrdinal("SellerID"));
                                var storedPassword = reader.GetString(reader.GetOrdinal("Password"));
                                if (VerifyPassword(password, storedPassword))
                                {
                                    Log.Information("Seller {Username} authenticated successfully", username);
                                    return (UserRole.Seller, null, sellerId);
                                }
                            }
                        }
                    }
                }

                Log.Warning("No matching user or seller found for Username={Username}", username);
                return (UserRole.Guest, null, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Authentication failed for username: {Username}", username);
                return (UserRole.Guest, null, null);
            }
        }


        private bool VerifyPassword(string inputPassword, string storedPassword)
        {
            // Placeholder for plain-text comparison (replace with hashing in production)
            return inputPassword == storedPassword;
        }

        public UserRole GetCurrentRole()
        {
            var roleString = _httpContextAccessor.HttpContext?.Session.GetString("Role");
            if (Enum.TryParse<UserRole>(roleString, out var role))
            {
                return role;
            }
            return UserRole.Guest;
        }

        public Guid? GetCurrentUserId()
        {
            var userIdString = _httpContextAccessor.HttpContext?.Session.GetString("UserId");
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            return null;
        }

        public Guid? GetCurrentSellerId()
        {
            var sellerIdString = _httpContextAccessor.HttpContext?.Session.GetString("SellerId");
            if (Guid.TryParse(sellerIdString, out var sellerId))
            {
                return sellerId;
            }
            return null;
        }
    }
}