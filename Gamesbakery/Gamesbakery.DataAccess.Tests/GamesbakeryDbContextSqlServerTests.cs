using Allure.Xunit.Attributes;
using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gamesbakery.DataAccess.Tests
{
    [Collection(TestCollections.SqlServer)]
    [AllureTag("Integration")]
    public class GamesbakeryDbContextSqlServerTests : IClassFixture<SqlServerDbContextFixture>
    {
        private readonly GamesbakeryDbContext _context;
        public GamesbakeryDbContextSqlServerTests(SqlServerDbContextFixture fixture)
        {
            _context = fixture.Context;
        }

        [AllureXunit(DisplayName = "ИГРА: ДОБАВЛЕНИЕ и ВЫБОР (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanAddAndRetrieveGame()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Arrange
                var category = new Category(Guid.NewGuid(), "Action", "Action games");
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var game = new Game(Guid.NewGuid(), category.Id, "Test Game", 29.99m,
                    DateTime.Now.AddYears(-1), "A test game", true, "Test Publisher");

                // Act
                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                var retrievedGame = await _context.Games.FindAsync(game.Id);

                // Assert
                Assert.NotNull(retrievedGame);
                Assert.Equal("Test Game", retrievedGame.Title);
                Assert.Equal(29.99m, retrievedGame.Price);

                var count = await _context.Games
                    .Where(g => g.Id == game.Id && g.Title == "Test Game" && g.Price == 29.99m)
                    .CountAsync();
                Assert.Equal(1, count);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [AllureXunit(DisplayName = "ИГРА: ОБНОВЛЕНИЕ (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanUpdateGame()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Arrange
                var category = new Category(Guid.NewGuid(), "RPG", "RPG games");
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var game = new Game(Guid.NewGuid(), category.Id, "Original Game", 19.99m,
                    DateTime.Now.AddYears(-2), "An original game", true, "Original Publisher");
                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                // Act
                game.UpdatePrice(24.99m);
                game.UpdateTitle("Updated Game");
                _context.Games.Update(game);
                await _context.SaveChangesAsync();

                var updatedGame = await _context.Games.FindAsync(game.Id);

                // Assert
                Assert.NotNull(updatedGame);
                Assert.Equal("Updated Game", updatedGame.Title);
                Assert.Equal(24.99m, updatedGame.Price);

                var count = await _context.Games
                    .Where(g => g.Id == game.Id && g.Title == "Updated Game" && g.Price == 24.99m)
                    .CountAsync();
                Assert.Equal(1, count);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }

        [AllureXunit(DisplayName = "ИГРА: УДАЛЕНИЕ (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanDeleteGame()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Arrange
                var category = new Category(Guid.NewGuid(), "Strategy", "Strategy games");
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var game = new Game(Guid.NewGuid(), category.Id, "Game to Delete", 39.99m,
                    DateTime.Now.AddYears(-3), "A game to delete", true, "Nisuev Alexander");
                _context.Games.Add(game);
                await _context.SaveChangesAsync();

                // Act
                _context.Games.Remove(game);
                await _context.SaveChangesAsync();

                var deletedGame = await _context.Games.FindAsync(game.Id);

                // Assert
                Assert.Null(deletedGame);

                var count = await _context.Games
                    .Where(g => g.Id == game.Id)
                    .CountAsync();
                Assert.Equal(0, count);
            }
            finally
            {
                await transaction.RollbackAsync();
            }
        }
    }
}
