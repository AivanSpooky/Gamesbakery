using System.Threading.Tasks;
using Allure.Commons;
using Allure.Xunit.Attributes;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.Repositories;
using Gamesbakery.DataAccess.Tests;
using Gamesbakery.DataAccess.Tests.Fixtures;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace Gamesbakery.BusinessLogic.Tests
{
    [Collection("GameServiceClassicCollection")]
    public class GameServiceClassicTests : IClassFixture<SqlServerDbContextFixture>
    {
        private readonly GamesbakeryDbContext _context;
        private readonly GameService _gameService;

        public GameServiceClassicTests(SqlServerDbContextFixture fixture)
        {
            _context = fixture.Context;
            var authService = new TestAuthenticationService();
            var categoryRepo = new CategoryRepository(_context);
            var gameRepo = new GameRepository(_context);
            _gameService = new GameService(gameRepo, categoryRepo, authService);
        }

        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("John Doe")]
        [AllureLink("Game API Docs", "https://dev.gamesbakery.com/api/games")]
        [AllureIssue("GAME-101")]
        [AllureXunit(DisplayName = "Добавление игры с корректными данными - классический тест")]
        [Trait("Category", "Unit")]
        public async Task AddGameAsync_ValidData_Classic_ReturnsGameDTO()
        {
            // Arrange
            if (!IsDatabaseAccessible())
                throw new InvalidOperationException("Test requires internet access to SQL Server.");

            var categoryId = Guid.NewGuid();
            var title = "Game Title";
            var price = 59.99m;
            var releaseDate = DateTime.UtcNow;
            var description = "Game Description";
            var originalPublisher = "Bethesda";
            var category = new Category(categoryId, "Action", "Action games");
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = await _gameService.AddGameAsync(categoryId, title, price, releaseDate, description, originalPublisher, false);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            Assert.Equal(price, result.Price);
        }

        private bool IsDatabaseAccessible()
        {
            try
            {
                using var conn = new SqlConnection(_context.Database.GetConnectionString());
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}