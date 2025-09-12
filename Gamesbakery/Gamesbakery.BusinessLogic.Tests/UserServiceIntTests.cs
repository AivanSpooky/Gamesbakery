using System;
using System.Threading.Tasks;
using Allure.Xunit.Attributes;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.BusinessLogic.Tests;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.Repositories;
using Gamesbakery.DataAccess.Tests;
using Gamesbakery.DataAccess.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gamesbakery.BusinessLogic.Tests
{
    [Collection(TestCollections.SqlServer)]
    [AllureTag("Integration")]
    public class UserServiceIntTests : IClassFixture<SqlServerDbContextFixture>
    {
        private readonly GamesbakeryDbContext _context;
        private readonly UserService _userService;

        public UserServiceIntTests(SqlServerDbContextFixture fixture)
        {
            _context = fixture.Context;
            var userRepo = new UserRepository(_context);
            var authService = new TestAuthenticationService();
            _userService = new UserService(userRepo, authService);
        }

        [AllureXunit(DisplayName = "ПОЛЬЗОВАТЕЛЬ: РЕГИСТРАЦИЯ (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanRegisterUser()
        {
            // Arrange
            var username = "TestUser";
            var email = "test@example.com";
            var password = "pass123";
            var country = "United States";

            // Act
            var result = await _userService.RegisterUserAsync(username, email, password, country);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Username);
            Assert.Equal(email, result.Email);

            // Fix: Use correct column names from your actual database schema
            var dbUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == result.Id);

            Assert.NotNull(dbUser);
            Assert.Equal(country, dbUser.Country);
        }

        [AllureXunit(DisplayName = "ПОЛЬЗОВАТЕЛЬ: БЛОКИРОВКА (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanBlockUser()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "BlockUser", "block@example.com", DateTime.UtcNow, "United States", "pass123", false, 100);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            await _userService.BlockUserAsync(user.Id);

            // Assert
            var dbUser = await _context.Users.FindAsync(user.Id);
            Assert.NotNull(dbUser);
            Assert.True(dbUser.IsBlocked);
        }
    }
}