using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess.Tests.Fixtures;

namespace Gamesbakery.DataAccess.Tests
{
    [Collection(TestCollections.InMemory)]
    public class GamesbakeryDbContextTests : IClassFixture<DbContextFixture>
    {
        private readonly GamesbakeryDbContext _context;

        public GamesbakeryDbContextTests(DbContextFixture fixture)
        {
            _context = fixture.Context;
        }

        [Fact(DisplayName = "ИГРА: ДОБАВЛЕНИЕ и ВЫБОР (InMemory)")]
        public async Task CanAddAndRetrieveGame()
        {
            // Arrange
            var category = new Category(Guid.NewGuid(), "Action", "Action games");
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var game = new Game(Guid.NewGuid(), category.Id,
                "Test Game",
                29.99m,
                DateTime.Now.AddYears(-1),
                "A test game",
                true,
                "Test Publisher");

            // Act
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            var retrievedGame = await _context.Games.FindAsync(game.Id);

            // Assert
            Assert.NotNull(retrievedGame);
            Assert.Equal("Test Game", retrievedGame.Title);
            Assert.Equal(29.99m, retrievedGame.Price);
        }

        [Fact(DisplayName = "ИГРА: ОБНОВЛЕНИЕ (InMemory)")]
        public async Task CanUpdateGame()
        {
            // Arrange
            var category = new Category(Guid.NewGuid(), "RPG", "RPG games");
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var game = new Game(Guid.NewGuid(),
                category.Id,
                "Original Game",
                19.99m,
                DateTime.Now.AddYears(-2),
                "An original game",
                true,
                "Original Publisher");

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
        }

        [Fact(DisplayName = "ИГРА: УДАЛЕНИЕ (InMemory)")]
        public async Task CanDeleteGame()
        {
            // Arrange
            var category = new Category(Guid.NewGuid(), "Strategy", "Strategy games");
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var game = new Game(Guid.NewGuid(),
                category.Id,
                "Game to Delete",
                39.99m,
                DateTime.Now.AddYears(-3),
                "A game to delete",
                true,
                "Nisuev Alexander");

            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            // Act
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();

            var deletedGame = await _context.Games.FindAsync(game.Id);

            // Assert
            Assert.Null(deletedGame);
        }
    }
}