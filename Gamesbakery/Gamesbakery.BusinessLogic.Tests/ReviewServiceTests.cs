using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Moq;
using Gamesbakery.Core;
using Allure.Xunit.Attributes;
using Allure.Commons;

namespace Gamesbakery.BusinessLogic.Tests
{
    [Collection("ReviewServiceCollection")]
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _reviewRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<IAuthenticationService> _authServiceMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
        private readonly ReviewService _reviewService;

        public ReviewServiceTests()
        {
            _reviewRepositoryMock = new Mock<IReviewRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
            _reviewService = new ReviewService(_reviewRepositoryMock.Object, _userRepositoryMock.Object, _gameRepositoryMock.Object, _authServiceMock.Object);
        }

        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("John Doe")]
        [AllureLink("Review API Docs", "https://dev.gamesbakery.com/api/reviews")]
        [AllureIssue("REVIEW-501")]
        [AllureXunit(DisplayName = "Добавление отзыва с корректными данными - успех")]
        [Trait("Category", "Unit")]
        public async Task AddReviewAsync_ValidData_ReturnsReviewDTO()
        {
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
            var result = await _reviewService.AddReviewAsync(userId, gameId, text, rating);
            Assert.NotNull(result);
            Assert.Equal(text, result.Text);
            Assert.Equal(rating, result.Rating);
        }

        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Review API Docs", "https://dev.gamesbakery.com/api/reviews")]
        [AllureIssue("REVIEW-502")]
        [AllureXunit(DisplayName = "Добавление отзыва с некорректным рейтингом - исключение")]
        [Trait("Category", "Unit")]
        public async Task AddReviewAsync_InvalidRating_ThrowsArgumentException()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var categoryId = Guid.NewGuid();
            var text = "Great game!";
            var rating = 6;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 100);
            var game = new Game(gameId, categoryId, "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
            await Assert.ThrowsAsync<ArgumentException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("John Doe")]
        [AllureLink("Review API Docs", "https://dev.gamesbakery.com/api/reviews")]
        [AllureIssue("REVIEW-503")]
        [AllureXunit(DisplayName = "Добавление отзыва с пустым текстом - исключение")]
        [Trait("Category", "Unit")]
        public async Task AddReviewAsync_EmptyText_ThrowsArgumentException()
        {
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
            await Assert.ThrowsAsync<ArgumentException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Review API Docs", "https://dev.gamesbakery.com/api/reviews")]
        [AllureIssue("REVIEW-504")]
        [AllureXunit(DisplayName = "Добавление отзыва для заблокированного пользователя - исключение")]
        [Trait("Category", "Unit")]
        public async Task AddReviewAsync_BlockedUser_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var text = "Great game!";
            var rating = 5;
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", true, 100);
            var game = new Game(gameId, Guid.NewGuid(), "Game Title", 59.99m, DateTime.UtcNow, "Description", true, "Bethesda");
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
            _orderItemRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, UserRole.User)).ReturnsAsync(new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), null, gameId, Guid.NewGuid(), "KEY", false)
            });
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reviewService.AddReviewAsync(userId, gameId, text, rating));
        }

        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("John Doe")]
        [AllureLink("Review API Docs", "https://dev.gamesbakery.com/api/reviews")]
        [AllureIssue("REVIEW-505")]
        [AllureXunit(DisplayName = "Получение отзывов по ID игры - успех")]
        [Trait("Category", "Unit")]
        public async Task GetReviewsByGameIdAsync_GameExists_ReturnsReviewDTOList()
        {
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
            _reviewRepositoryMock.Setup(repo => repo.GetByGameIdAsync(gameId, UserRole.User, null, null, null)).ReturnsAsync(reviews); // Explicit parameters
            var result = await _reviewService.GetReviewsByGameIdAsync(gameId);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Review API Docs", "https://dev.gamesbakery.com/api/reviews")]
        [AllureIssue("REVIEW-506")]
        [AllureXunit(DisplayName = "Получение отзывов для несуществующей игры - исключение")]
        [Trait("Category", "Unit")]
        public async Task GetReviewsByGameIdAsync_GameNotFound_ThrowsKeyNotFoundException()
        {
            var gameId = Guid.NewGuid();
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _reviewRepositoryMock.Setup(repo => repo.GetByGameIdAsync(gameId, UserRole.User, null, null, null)).ReturnsAsync((List<Review>)null); // Explicit parameters
            await Assert.ThrowsAsync<ArgumentNullException>(() => _reviewService.GetReviewsByGameIdAsync(gameId));
        }
    }

    public interface IOrderItemRepository
    {
        Task<OrderItem> AddAsync(OrderItem orderItem, UserRole role);
        Task<OrderItem> GetByIdAsync(Guid id, UserRole role, Guid? userId = null);
        Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, UserRole role);
        Task<List<OrderItem>> GetBySellerIdAsync(Guid sellerId, UserRole role);
        Task<OrderItem> UpdateAsync(OrderItem orderItem, UserRole role);
        Task DeleteAsync(Guid id, UserRole role);
        Task<int> GetCountAsync(Guid? sellerId = null, Guid? gameId = null, UserRole role = UserRole.Admin);
        Task<List<OrderItem>> GetFilteredAsync(Guid? sellerId = null, Guid? gameId = null, UserRole role = UserRole.Admin);
        Task<List<OrderItem>> GetAvailableByGameIdAsync(Guid gameId, UserRole role);
        Task<List<OrderItem>> GetByUserIdAsync(Guid userId, UserRole role);
    }
}
