using System;
using System.Threading.Tasks;
using Allure.Xunit.Attributes;
using Gamesbakery.BusinessLogic.Services;
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
    public class GameServiceIT : IClassFixture<SqlServerDbContextFixture>
    {
        private readonly GamesbakeryDbContext _context;
        private readonly GameService _gameService;

        public GameServiceIT(SqlServerDbContextFixture fixture)
        {
            _context = fixture.Context;
            var categoryRepo = new CategoryRepository(_context);
            var gameRepo = new GameRepository(_context);
            var authService = new TestAuthenticationService();
            _gameService = new GameService(gameRepo, categoryRepo, authService);
        }

        [AllureXunit(DisplayName = "ИГРА: ДОБАВЛЕНИЕ СЕРВИСОМ (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanAddGameViaService()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category(categoryId, "Action", "Action games");
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.AddGameAsync(categoryId, "Service Test Game", 49.99m, DateTime.UtcNow, "Service Desc", "Service Pub");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Service Test Game", result.Title);
            var dbGame = await _context.Games.FindAsync(result.Id);
            Assert.NotNull(dbGame);
            Assert.Equal(49.99m, dbGame.Price);
        }

        [AllureXunit(DisplayName = "ИГРА: ОБНОВЛЕНИЕ СТАТУСА ПРОДАЖИ СЕРВИСОМ (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanSetGameForSaleViaService()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category(categoryId, "RPG", "RPG games");
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            var game = new Game(Guid.NewGuid(), categoryId, "Sale Game", 39.99m, DateTime.UtcNow, "Sale Desc", true, "Sale Pub");
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.SetGameForSaleAsync(game.Id, false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsForSale);
            var dbGame = await _context.Games.FindAsync(game.Id);
            Assert.NotNull(dbGame);
            Assert.False(dbGame.IsForSale);
        }

        [AllureXunit(DisplayName = "ИГРА: ПОЛУЧЕНИЕ ВСЕХ ИГР СЕРВИСОМ (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanGetAllGamesViaService()
        {
            // Очистите базу перед тестом
            _context.Games.RemoveRange(_context.Games);
            _context.Categories.RemoveRange(_context.Categories);
            await _context.SaveChangesAsync();

            // Arrange
            var categoryId = Guid.NewGuid();
            var category = new Category(categoryId, "Strategy", "Strategy games");
            _context.Categories.Add(category);
            var game1 = new Game(Guid.NewGuid(), categoryId, "Game 1", 29.99m, DateTime.UtcNow, "Desc 1", true, "Pub 1");
            var game2 = new Game(Guid.NewGuid(), categoryId, "Game 2", 19.99m, DateTime.UtcNow, "Desc 2", true, "Pub 2");
            _context.Games.AddRange(game1, game2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.GetAllGamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.Title == "Game 1");
            Assert.Contains(result, g => g.Title == "Game 2");
        }
    }
}