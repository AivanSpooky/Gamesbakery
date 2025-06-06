﻿using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Moq;
using Xunit;
using Gamesbakery.Core;

namespace Gamesbakery.Tests
{
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<IAuthenticationService> _authServiceMock;
        private readonly ReviewService _reviewService;

        public ReviewServiceTests()
        {
            _reviewRepositoryMock = new Mock<IReviewRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _reviewService = new ReviewService(_reviewRepositoryMock.Object, _userRepositoryMock.Object, _gameRepositoryMock.Object, _authServiceMock.Object);
        }

        [Fact(DisplayName = "Добавление отзыва с корректными данными - успех")]
        public async Task AddReviewAsync_ValidData_ReturnsReviewDTO()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var text = "Great game!";
            var rating = 5;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 100);
            var game = new Game(gameId, categoryId, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");
            var review = new Review(Guid.NewGuid(), userId, gameId, text, rating, DateTime.UtcNow);

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
            _reviewRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Review>(), UserRole.User)).ReturnsAsync(review);

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
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var text = "Great game!";
            var rating = 6; // Рейтинг должен быть от 1 до 5
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 100);
            var game = new Game(gameId, categoryId, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [Fact(DisplayName = "Добавление отзыва с пустым текстом - исключение")]
        public async Task AddReviewAsync_EmptyText_ThrowsArgumentException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var text = "";
            var rating = 5;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 100);
            var game = new Game(gameId, categoryId, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [Fact(DisplayName = "Добавление отзыва для заблокированного пользователя - исключение")]
        public async Task AddReviewAsync_BlockedUser_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var text = "Great game!";
            var rating = 5;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", true, 100);
            var game = new Game(gameId, categoryId, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [Fact(DisplayName = "Получение отзывов по ID игры - успех")]
        public async Task GetReviewsByGameIdAsync_GameExists_ReturnsReviewDTOList()
        {
            // Arrange
            var gameId = Guid.NewGuid();
            var userId1 = Guid.NewGuid();
            var userId2 = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var game = new Game(gameId, categoryId, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");
            var reviews = new List<Review>
            {
                new Review(Guid.NewGuid(), userId1, gameId, "Great game!", 5, DateTime.UtcNow),
                new Review(Guid.NewGuid(), userId2, gameId, "Not bad", 3, DateTime.UtcNow)
            };

            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
            _reviewRepositoryMock.Setup(repo => repo.GetByGameIdAsync(gameId, UserRole.User)).ReturnsAsync(reviews);

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
            var gameId = Guid.NewGuid();

            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync((Game)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _reviewService.GetReviewsByGameIdAsync(gameId));
        }
    }
}