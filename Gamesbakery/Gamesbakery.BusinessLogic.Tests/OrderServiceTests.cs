using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Moq;
using Allure.Xunit.Attributes;
using Allure.Commons;

namespace Gamesbakery.BusinessLogic.Tests
{
    [Collection("OrderServiceCollection")]
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<Core.Repositories.IOrderItemRepository> _orderItemRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<ISellerRepository> _sellerRepositoryMock;
        private readonly Mock<IAuthenticationService> _authServiceMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderItemRepositoryMock = new Mock<Core.Repositories.IOrderItemRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _sellerRepositoryMock = new Mock<ISellerRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _orderService = new OrderService(_orderRepositoryMock.Object, _orderItemRepositoryMock.Object, _userRepositoryMock.Object, _gameRepositoryMock.Object, _sellerRepositoryMock.Object, _authServiceMock.Object);
        }

        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("John Doe")]
        [AllureLink("Order API Docs", "https://dev.gamesbakery.com/api/orders")]
        [AllureIssue("ORDER-301")]
        [AllureXunit(DisplayName = "Создание заказа с недостаточным балансом - исключение")]
        [Trait("Category", "Unit")]
        public async Task CreateOrderAsync_InsufficientBalance_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var orderItemIds = new List<Guid> { orderItemId };
            var categoryId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 20);
            var game = new Game(gameId, categoryId, "Game 1", 50m, DateTime.UtcNow, "Desc 1", true, "Bethesda");
            var seller = new Seller(sellerId, "Seller1", DateTime.UtcNow, 4.5, "pass");
            var orderItem = new OrderItem(orderItemId, null, gameId, sellerId, null, false);
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, UserRole.User, userId)).ReturnsAsync(orderItem);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
            _sellerRepositoryMock.Setup(repo => repo.GetAllAsync(UserRole.User)).ReturnsAsync(new List<Seller> { seller });
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, orderItemIds));
        }

        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Order API Docs", "https://dev.gamesbakery.com/api/orders")]
        [AllureIssue("ORDER-302")]
        [AllureXunit(DisplayName = "Создание заказа для заблокированного пользователя - исключение")]
        [Trait("Category", "Unit")]
        public async Task CreateOrderAsync_BlockedUser_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var gameIds = new List<Guid> { gameId };
            var categoryId = Guid.NewGuid();
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", true, 200);
            var game = new Game(gameId, categoryId, "Game 1", 50m, DateTime.UtcNow, "Desc 1", true, "Bethesda");
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("John Doe")]
        [AllureLink("Order API Docs", "https://dev.gamesbakery.com/api/orders")]
        [AllureIssue("ORDER-303")]
        [AllureXunit(DisplayName = "Создание заказа с несуществующей игрой - исключение")]
        [Trait("Category", "Unit")]
        public async Task CreateOrderAsync_GameNotFound_ThrowsKeyNotFoundException()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var gameIds = new List<Guid> { gameId };
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 200);
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync((Game)null);
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Order API Docs", "https://dev.gamesbakery.com/api/orders")]
        [AllureIssue("ORDER-304")]
        [AllureXunit(DisplayName = "Создание заказа с игрой, не доступной для продажи - исключение")]
        [Trait("Category", "Unit")]
        public async Task CreateOrderAsync_GameNotForSale_ThrowsInvalidOperationException()
        {
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var orderItemId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var orderItemIds = new List<Guid> { orderItemId };
            var categoryId = Guid.NewGuid();
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 200);
            var game = new Game(gameId, categoryId, "Game 1", 50m, DateTime.UtcNow, "Desc 1", false, "Bethesda");
            var orderItem = new OrderItem(orderItemId, null, gameId, sellerId, null, false);
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, UserRole.User, userId)).ReturnsAsync(orderItem);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, orderItemIds));
        }

        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("John Doe")]
        [AllureLink("Order API Docs", "https://dev.gamesbakery.com/api/orders")]
        [AllureIssue("ORDER-305")]
        [AllureXunit(DisplayName = "Получение заказов по ID пользователя - успех")]
        [Trait("Category", "Unit")]
        public async Task GetOrdersByUserIdAsync_ReturnsOrderDTOList()
        {
            var userId = Guid.NewGuid();
            var orders = new List<Order>
            {
                new Order(Guid.NewGuid(), userId, DateTime.UtcNow, 100m, "Pending", false, false),
                new Order(Guid.NewGuid(), userId, DateTime.UtcNow, 50m, "Completed", true, false)
            };
            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, UserRole.User)).ReturnsAsync(orders);
            var result = await _orderService.GetOrdersByUserIdAsync(userId);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Order API Docs", "https://dev.gamesbakery.com/api/orders")]
        [AllureIssue("ORDER-306")]
        [AllureXunit(DisplayName = "Установка ключа для чужого OrderItem - исключение")]
        [Trait("Category", "Unit")]
        public async Task SetOrderItemKeyAsync_WrongSeller_ThrowsInvalidOperationException()
        {
            var orderItemId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var wrongSellerId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var key = "KEY-123";
            var orderItem = new OrderItem(orderItemId, orderId, gameId, sellerId, null, false);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Seller);
            _authServiceMock.Setup(auth => auth.GetCurrentSellerId()).Returns(wrongSellerId);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, UserRole.Seller, wrongSellerId)).ReturnsAsync(orderItem);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.SetOrderItemKeyAsync(orderItemId, key, wrongSellerId));
        }

        [AllureSeverity(SeverityLevel.normal)]
        [AllureOwner("John Doe")]
        [AllureLink("Order API Docs", "https://dev.gamesbakery.com/api/orders")]
        [AllureIssue("ORDER-307")]
        [AllureXunit(DisplayName = "Установка некорректного ключа - исключение")]
        [Trait("Category", "Unit")]
        public async Task SetOrderItemKeyAsync_InvalidKey_ThrowsArgumentException()
        {
            var orderItemId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var invalidKey = "";
            var orderItem = new OrderItem(orderItemId, orderId, gameId, sellerId, null, false);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Seller);
            _authServiceMock.Setup(auth => auth.GetCurrentSellerId()).Returns(sellerId);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, UserRole.Seller, sellerId)).ReturnsAsync(orderItem);
            await Assert.ThrowsAsync<ArgumentException>(() => _orderService.SetOrderItemKeyAsync(orderItemId, invalidKey, sellerId));
        }

        [AllureSeverity(SeverityLevel.critical)]
        [AllureOwner("Jane Smith")]
        [AllureLink("Order API Docs", "https://dev.gamesbakery.com/api/orders")]
        [AllureIssue("ORDER-308")]
        [AllureXunit(DisplayName = "Получение OrderItems по SellerID - успех")]
        [Trait("Category", "Unit")]
        public async Task GetOrderItemsBySellerIdAsync_ReturnsOrderItemDTOList()
        {
            var sellerId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();
            var orderItems = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), orderId, gameId1, sellerId, null, false),
                new OrderItem(Guid.NewGuid(), orderId, gameId2, sellerId, "KEY-456", false)
            };
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Seller);
            _orderItemRepositoryMock.Setup(repo => repo.GetBySellerIdAsync(sellerId, UserRole.Seller)).ReturnsAsync(orderItems);
            var result = await _orderService.GetOrderItemsBySellerIdAsync(sellerId);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("KEY-456", result[1].Key);
        }
    }
}