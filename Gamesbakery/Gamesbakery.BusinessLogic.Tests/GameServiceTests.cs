using Allure.Commons;
using Allure.Xunit.Attributes;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.BusinessLogic.Tests.Patterns;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Moq;

namespace Gamesbakery.BusinessLogic.Tests
{
    [AllureOwner("Tinkoff")]
    [AllureTag("TAG-ALL")]
    [AllureEpic("TestEpic")]
    [AllureParentSuite("AllTests")]
    [AllureSuite("Suite Name")]
    [AllureSubSuite("Subsuite Name")]
    [AllureSeverity(SeverityLevel.minor)]
    [AllureLink("Google", "https://google.com")]
    [AllureLink("Tinkoff", "https://tinkoff.ru")]
    [Collection("GameServiceCollection")]
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IAuthenticationService> _authServiceMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _gameRepositoryMock = new Mock<IGameRepository>();
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _gameService = new GameService(_gameRepositoryMock.Object, _categoryRepositoryMock.Object, _authServiceMock.Object);
        }

        [AllureXunit(DisplayName = "Добавление игры с корректными данными - успех")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("John Doe")]
        [AllureLink("Game API Docs", "https://dev.gamesbakery.com/api/games")]
        [AllureIssue("GAME-101")]
        [AllureTag("Game")]
        [Trait("Category", "Unit")]
        public async Task AddGameAsync_ValidData_ReturnsGameDTO()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var title = "Game Title";
            var price = 59.99m;
            var releaseDate = DateTime.UtcNow;
            var description = "Game Description";
            var originalPublisher = "Bethesda";
            var category = new Category(categoryId, "Action", "Action games");
            var gameId = Guid.NewGuid();
            var game = GameObjectMother.ValidGame(gameId, categoryId, title, price, releaseDate, description, true, originalPublisher);

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, UserRole.Admin)).ReturnsAsync(category);
            _gameRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Game>(), UserRole.Admin)).ReturnsAsync(game);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);

            // Act
            var result = await _gameService.AddGameAsync(categoryId, title, price, releaseDate, description, originalPublisher);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(title, result.Title);
            Assert.Equal(price, result.Price);
            Assert.Equal(originalPublisher, result.OriginalPublisher);
        }

        [AllureXunit(DisplayName = "Добавление игры с несуществующей категорией - исключение")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Game API Docs", "https://dev.gamesbakery.com/api/games")]
        [AllureIssue("GAME-102")]
        [Trait("Category", "Unit")]
        public async Task AddGameAsync_CategoryNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var title = "Game Title";
            var price = 59.99m;
            var releaseDate = DateTime.UtcNow;
            var description = "Game Description";
            var originalPublisher = "Bethesda";
            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, UserRole.Admin)).ReturnsAsync((Category)null);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _gameService.AddGameAsync(categoryId, title, price, releaseDate, description, originalPublisher));
        }

        [AllureXunit(DisplayName = "Добавление игры с отрицательной ценой - исключение")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("John Doe")]
        [AllureLink("Game API Docs", "https://dev.gamesbakery.com/api/games")]
        [AllureIssue("GAME-103")]
        [Trait("Category", "Unit")]
        public async Task AddGameAsync_NegativePrice_ThrowsArgumentException()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var title = "Game Title";
            var price = -10m;
            var releaseDate = DateTime.UtcNow;
            var description = "Game Description";
            var originalPublisher = "Bethesda";
            var category = new Category(categoryId, "Action", "Action games");
            var gameId = Guid.NewGuid();
            var game = GameObjectMother.InvalidPriceGame(gameId, categoryId, title, 0m, releaseDate, description, true, originalPublisher); // Valid base, price overridden

            _categoryRepositoryMock.Setup(repo => repo.GetByIdAsync(categoryId, UserRole.Admin)).ReturnsAsync(category);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);
            _gameRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Game>(), UserRole.Admin)).Throws<ArgumentException>(); // Mock to throw exception

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _gameService.AddGameAsync(categoryId, title, price, releaseDate, description, originalPublisher));
        }

        [AllureXunit(DisplayName = "Получение всех игр - успех")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Game API Docs", "https://dev.gamesbakery.com/api/games")]
        [AllureIssue("GAME-104")]
        [Trait("Category", "Unit")]
        public async Task GetAllGamesAsync_ReturnsGameDTOList()
        {
            // Arrange
            var categoryId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();
            var games = new List<Game>
            {
                GameObjectMother.ValidGame(gameId1, categoryId, "Game 1", 59.99m, DateTime.UtcNow, "Desc 1", true, "Bethesda"),
                GameObjectMother.ValidGame(gameId2, categoryId, "Game 2", 29.99m, DateTime.UtcNow, "Desc 2", true, "Valve")
            };

            _gameRepositoryMock.Setup(repo => repo.GetAllAsync(UserRole.User)).ReturnsAsync(games);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);

            // Act
            var result = await _gameService.GetAllGamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [AllureXunit(DisplayName = "Установка статуса продажи игры - успех")]
        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("John Doe")]
        [AllureLink("Game API Docs", "https://dev.gamesbakery.com/api/games")]
        [AllureIssue("GAME-105")]
        [Trait("Category", "Unit")]
        public async Task SetGameForSaleAsync_ValidData_Success()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var game = GameObjectMother.ValidGame(gameId, categoryId, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");

            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.Admin)).ReturnsAsync(game);
            _gameRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Game>(), UserRole.Admin)).ReturnsAsync(game);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);

            // Act
            var result = await _gameService.SetGameForSaleAsync(gameId, false);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsForSale);
        }

        [AllureXunit(DisplayName = "Установка статуса продажи для несуществующей игры - исключение")]
        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Game API Docs", "https://dev.gamesbakery.com/api/games")]
        [AllureIssue("GAME-106")]
        [Trait("Category", "Unit")]
        public async Task SetGameForSaleAsync_GameNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.Admin)).ReturnsAsync((Game)null);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _gameService.SetGameForSaleAsync(gameId, false));
        }
    }
}
