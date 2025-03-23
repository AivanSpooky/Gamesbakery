using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Moq;
using Xunit;

namespace Gamesbakery.Tests
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _gameRepositoryMock = new Mock<IGameRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _gameService = new GameService(_gameRepositoryMock.Object, _categoryRepositoryMock.Object);
        }

        [Fact(DisplayName = "Добавление игры с корректными данными - успех")]
        public async Task AddGameAsync_ValidData_ReturnsGameDTO()
        {
            // Arrange
            var categoryId = 1;
            var title = "Game Title";
            var price = 59.99m;
            var releaseDate = DateTime.UtcNow;
            var description = "Game Description";
            var originalPublisher = "Bethesda";
            var category = new Category(categoryId, "Action", "Action games");
            var game = new Game(1, categoryId, title, price, releaseDate, description, true, originalPublisher);

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync(category);
            _gameRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Game>())).ReturnsAsync(game);

            // Act
            var result = await _gameService.AddGameAsync(categoryId, title, price, releaseDate, description, originalPublisher);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            Assert.Equal(price, result.Price);
            Assert.Equal(originalPublisher, result.OriginalPublisher);
        }

        [Fact(DisplayName = "Добавление игры с несуществующей категорией - исключение")]
        public async Task AddGameAsync_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var categoryId = 1;
            var title = "Game Title";
            var price = 59.99m;
            var releaseDate = DateTime.UtcNow;
            var description = "Game Description";
            var originalPublisher = "Bethesda";

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync((Category)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _gameService.AddGameAsync(categoryId, title, price, releaseDate, description, originalPublisher));
        }

        [Fact(DisplayName = "Добавление игры с отрицательной ценой - исключение")]
        public async Task AddGameAsync_NegativePrice_ThrowsArgumentException()
        {
            // Arrange
            var categoryId = 1;
            var title = "Game Title";
            var price = -10m;
            var releaseDate = DateTime.UtcNow;
            var description = "Game Description";
            var originalPublisher = "Bethesda";
            var category = new Category(categoryId, "Action", "Action games");

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId)).ReturnsAsync(category);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _gameService.AddGameAsync(categoryId, title, price, releaseDate, description, originalPublisher));
        }

        [Fact(DisplayName = "Получение всех игр - успех")]
        public async Task GetAllGamesAsync_ReturnsGameDTOList()
        {
            // Arrange
            var games = new List<Game>
            {
                new Game(1, 1, "Game 1", 59.99m, DateTime.UtcNow, "Desc 1", true, "Bethesda"),
                new Game(2, 1, "Game 2", 29.99m, DateTime.UtcNow, "Desc 2", true, "Valve")
            };
            _gameRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(games);

            // Act
            var result = await _gameService.GetAllGamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact(DisplayName = "Установка статуса продажи игры - успех")]
        public async Task SetGameForSaleAsync_ValidData_Success()
        {
            // Arrange
            var gameId = 1;
            var game = new Game(gameId, 1, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Game>())).ReturnsAsync(game);

            // Act
            var result = await _gameService.SetGameForSaleAsync(gameId, false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsForSale);
        }

        [Fact(DisplayName = "Установка статуса продажи для несуществующей игры - исключение")]
        public async Task SetGameForSaleAsync_GameNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var gameId = 1;
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _gameService.SetGameForSaleAsync(gameId, false));
        }
    }
}