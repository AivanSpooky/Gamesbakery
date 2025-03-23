using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Moq;
using Xunit;

namespace Gamesbakery.Tests
{
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly ReviewService _reviewService;

        public ReviewServiceTests()
        {
            _reviewRepositoryMock = new Mock<IReviewRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _reviewService = new ReviewService(_reviewRepositoryMock.Object, _userRepositoryMock.Object, _gameRepositoryMock.Object);
        }

        [Fact(DisplayName = "Добавление отзыва с корректными данными - успех")]
        public async Task AddReviewAsync_ValidData_ReturnsReviewDTO()
        {
            // Arrange
            var userId = 1;
            var gameId = 1;
            var text = "Great game!";
            var rating = 5;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", false, 100);
            var game = new Game(gameId, 1, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");
            var review = new Review(1, userId, gameId, text, rating, DateTime.UtcNow);

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(game);
            _reviewRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Review>())).ReturnsAsync(review);

            // Act
            var result = await _reviewService.AddReviewAsync(userId, gameId, text, rating);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(text, result.Text);
            Assert.Equal(rating, result.Rating);
        }

        [Fact(DisplayName = "Добавление отзыва с некорректным рейтингом - исключение")]
        public async Task AddReviewAsync_InvalidRating_ThrowsArgumentException()
        {
            // Arrange
            var userId = 1;
            var gameId = 1;
            var text = "Great game!";
            var rating = 6; // Рейтинг должен быть от 1 до 5
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", false, 100);
            var game = new Game(gameId, 1, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [Fact(DisplayName = "Добавление отзыва с пустым текстом - исключение")]
        public async Task AddReviewAsync_EmptyText_ThrowsArgumentException()
        {
            // Arrange
            var userId = 1;
            var gameId = 1;
            var text = "";
            var rating = 5;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", false, 100);
            var game = new Game(gameId, 1, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [Fact(DisplayName = "Добавление отзыва для заблокированного пользователя - исключение")]
        public async Task AddReviewAsync_BlockedUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var gameId = 1;
            var text = "Great game!";
            var rating = 5;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", true, 100);
            var game = new Game(gameId, 1, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [Fact(DisplayName = "Получение отзывов по ID игры - успех")]
        public async Task GetReviewsByGameIdAsync_GameExists_ReturnsReviewDTOList()
        {
            // Arrange
            var gameId = 1;
            var game = new Game(gameId, 1, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");
            var reviews = new List<Review>
            {
                new Review(1, 1, gameId, "Great game!", 5, DateTime.UtcNow),
                new Review(2, 2, gameId, "Not bad", 3, DateTime.UtcNow)
            };

            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync(game);
            _reviewRepositoryMock.Setup(repo => repo.GetByGameIdAsync(gameId)).ReturnsAsync(reviews);

            // Act
            var result = await _reviewService.GetReviewsByGameIdAsync(gameId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact(DisplayName = "Получение отзывов для несуществующей игры - исключение")]
        public async Task GetReviewsByGameIdAsync_GameNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var gameId = 1;
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId)).ReturnsAsync((Game)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _reviewService.GetReviewsByGameIdAsync(gameId));
        }
    }
}