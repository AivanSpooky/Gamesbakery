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

        //[AllureXunit(DisplayName = "ПОЛЬЗОВАТЕЛЬ: РЕГИСТРАЦИЯ (SQL Server)")]
        //[Trait("Category", "Integration")]
        //public async Task CanRegisterUser()
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Arrange: используем уникальное имя
        //        var username = $"TestUser_{Guid.NewGuid():N}[0..8]";
        //        var email = $"test{Guid.NewGuid():N}@example.com";
        //        var password = "pass123";
        //        var country = "United States";

        //        // Act
        //        var result = await _userService.RegisterUserAsync(username, email, password, country);

        //        // ИСПРАВЛЕНО: проверяем, что метод выполнился без ошибки
        //        Assert.NotNull(result);
        //        Assert.Equal(username, result.Username);
        //        Assert.Equal(email, result.Email);

        //        // Проверяем, что пользователь сохранился в базе
        //        var dbUser = await _context.Users
        //            .FirstOrDefaultAsync(u => u.Id == result.Id);
        //        Assert.NotNull(dbUser);
        //        Assert.Equal(username, dbUser.Username);
        //        Assert.Equal(email, dbUser.Email);
        //        Assert.Equal(country, dbUser.Country);
        //        Assert.False(dbUser.IsBlocked);
        //        Assert.True(dbUser.Balance >= 0);
        //    }
        //    finally
        //    {
        //        await transaction.RollbackAsync();
        //    }
        //}

        //[AllureXunit(DisplayName = "ПОЛЬЗОВАТЕЛЬ: БЛОКИРОВКА (SQL Server)")]
        //[Trait("Category", "Integration")]
        //public async Task CanBlockUser()
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Arrange: создаем пользователя через сервис
        //        var userId = Guid.NewGuid();
        //        var username = $"BlockUser_{Guid.NewGuid():N}[0..8]";
        //        var email = $"block{Guid.NewGuid():N}@example.com";
        //        var password = "pass123";
        //        var country = "United States";

        //        // ИСПРАВЛЕНО: создаем пользователя через сервис вместо прямого добавления в БД
        //        var createdUser = await _userService.RegisterUserAsync(username, email, password, country);
        //        Assert.NotNull(createdUser);

        //        // Act: блокируем пользователя
        //        await _userService.BlockUserAsync(createdUser.Id);

        //        // Assert: проверяем, что пользователь заблокирован
        //        var dbUser = await _context.Users.FindAsync(createdUser.Id);
        //        Assert.NotNull(dbUser);
        //        Assert.True(dbUser.IsBlocked);
        //        Assert.Equal(username, dbUser.Username);
        //    }
        //    finally
        //    {
        //        await transaction.RollbackAsync();
        //    }
        //}
    }
}
