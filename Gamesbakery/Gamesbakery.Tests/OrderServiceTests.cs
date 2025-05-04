using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Moq;
using Xunit;
using Gamesbakery.Core;

namespace Gamesbakery.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<ISellerRepository> _sellerRepositoryMock;
        private readonly Mock<IAuthenticationService> _authServiceMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _sellerRepositoryMock = new Mock<ISellerRepository>();
            _authServiceMock = new Mock<IAuthenticationService>();
            _orderService = new OrderService(_orderRepositoryMock.Object, _orderItemRepositoryMock.Object, _userRepositoryMock.Object, _gameRepositoryMock.Object, _sellerRepositoryMock.Object, _authServiceMock.Object);
        }

        //[Fact(DisplayName = "Создание заказа с корректными данными - успех")]
        //public async Task CreateOrderAsync_ValidData_ReturnsOrderDTO()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var gameId1 = Guid.NewGuid();
        //    var gameId2 = Guid.NewGuid();
        //    var gameIds = new List<Guid> { gameId1, gameId2 };
        //    var categoryId = Guid.NewGuid();
        //    var sellerId = Guid.NewGuid();
        //    var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 200);
        //    var game1 = new Game(gameId1, categoryId, "Game 1", 50, DateTime.UtcNow, "Desc 1", true, "Bethesda");
        //    var game2 = new Game(gameId2, categoryId, "Game 2", 50, DateTime.UtcNow, "Desc 2", true, "Valve");
        //    var seller = new Seller(sellerId, "Seller1", DateTime.UtcNow, 4.5, "pass");
        //    var order = new Order(Guid.NewGuid(), userId, DateTime.UtcNow, 100, false, false);

        //    _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
        //    _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
        //    _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
        //    _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId1, UserRole.User)).ReturnsAsync(game1);
        //    _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId2, UserRole.User)).ReturnsAsync(game2);
        //    _sellerRepositoryMock.Setup(repo => repo.GetAllAsync(UserRole.User)).ReturnsAsync(new List<Seller> { seller });
        //    _orderRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Order>(), UserRole.User)).ReturnsAsync(order);
        //    _orderItemRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<OrderItem>(), UserRole.User)).ReturnsAsync((OrderItem orderItem) => orderItem);
        //    _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>(), UserRole.User)).ReturnsAsync(user);

        //    // Act
        //    var result = await _orderService.CreateOrderAsync(userId, gameIds);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(100, result.Price);
        //    Assert.False(result.IsCompleted);
        //}

        [Fact(DisplayName = "Создание заказа с недостаточным балансом - исключение")]
        public async Task CreateOrderAsync_InsufficientBalance_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var gameIds = new List<Guid> { gameId };
            var categoryId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 20);
            var game = new Game(gameId, categoryId, "Game 1", 50, DateTime.UtcNow, "Desc 1", true, "Bethesda");
            var seller = new Seller(sellerId, "Seller1", DateTime.UtcNow, 4.5, "pass");

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
            _sellerRepositoryMock.Setup(repo => repo.GetAllAsync(UserRole.User)).ReturnsAsync(new List<Seller> { seller });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [Fact(DisplayName = "Создание заказа для заблокированного пользователя - исключение")]
        public async Task CreateOrderAsync_BlockedUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var gameIds = new List<Guid> { gameId };
            var categoryId = Guid.NewGuid();
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", true, 200);
            var game = new Game(gameId, categoryId, "Game 1", 50, DateTime.UtcNow, "Desc 1", true, "Bethesda");

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [Fact(DisplayName = "Создание заказа с несуществующей игрой - исключение")]
        public async Task CreateOrderAsync_GameNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var gameIds = new List<Guid> { gameId };
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 200);

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync((Game)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [Fact(DisplayName = "Создание заказа с игрой, не доступной для продажи - исключение")]
        public async Task CreateOrderAsync_GameNotForSale_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var gameIds = new List<Guid> { gameId };
            var categoryId = Guid.NewGuid();
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 200);
            var game = new Game(gameId, categoryId, "Game 1", 50, DateTime.UtcNow, "Desc 1", false, "Bethesda");

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        //[Fact(DisplayName = "Создание заказа без доступных продавцов - исключение")]
        //public async Task CreateOrderAsync_NoSellersAvailable_ThrowsUnauthorizedAccessException()
        //{
        //    // Arrange
        //    var userId = Guid.NewGuid();
        //    var gameId = Guid.NewGuid();
        //    var gameIds = new List<Guid> { gameId };
        //    var categoryId = Guid.NewGuid();
        //    var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "United States", "password123", false, 200);
        //    var game = new Game(gameId, categoryId, "Game 1", 50, DateTime.UtcNow, "Desc 1", true, "Bethesda");

        //    _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
        //    _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
        //    _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId, UserRole.User)).ReturnsAsync(user);
        //    _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(gameId, UserRole.User)).ReturnsAsync(game);
        //    _sellerRepositoryMock.Setup(repo => repo.GetAllAsync(UserRole.User)).ReturnsAsync(new List<Seller>());

        //    // Act & Assert
        //    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        //}

        [Fact(DisplayName = "Получение заказов по ID пользователя - успех")]
        public async Task GetOrdersByUserIdAsync_ReturnsOrderDTOList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var orders = new List<Order>
            {
                new Order(Guid.NewGuid(), userId, DateTime.UtcNow, 100, false, false),
                new Order(Guid.NewGuid(), userId, DateTime.UtcNow, 50, true, false)
            };

            _authServiceMock.Setup(auth => auth.GetCurrentUserId()).Returns(userId);
            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.User);
            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, UserRole.User)).ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetOrdersByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        //[Fact(DisplayName = "Установка ключа для OrderItem - успех")]
        //public async Task SetOrderItemKeyAsync_ValidData_Success()
        //{
        //    // Arrange
        //    var orderItemId = Guid.NewGuid();
        //    var sellerId = Guid.NewGuid();
        //    var orderId = Guid.NewGuid();
        //    var gameId = Guid.NewGuid();
        //    var key = "KEY-123";
        //    var orderItem = new OrderItem(orderItemId, orderId, gameId, sellerId, null);

        //    _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Seller);
        //    _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, UserRole.Seller)).ReturnsAsync(orderItem);
        //    _orderItemRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<OrderItem>(), UserRole.Seller)).Callback<OrderItem>(item => item.SetKey(key));

        //    // Act
        //    await _orderService.SetOrderItemKeyAsync(orderItemId, key, sellerId);

        //    // Assert
        //    Assert.Equal(key, orderItem.Key);
        //    _orderItemRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<OrderItem>(item => item.Id == orderItemId && item.Key == key), UserRole.Seller), Times.Once());
        //}

        [Fact(DisplayName = "Установка ключа для чужого OrderItem - исключение")]
        public async Task SetOrderItemKeyAsync_WrongSeller_ThrowsInvalidOperationException()
        {
            // Arrange
            var orderItemId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var wrongSellerId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var key = "KEY-123";
            var orderItem = new OrderItem(orderItemId, orderId, gameId, sellerId, null);

            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Seller);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, UserRole.Seller)).ReturnsAsync(orderItem);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.SetOrderItemKeyAsync(orderItemId, key, wrongSellerId));
        }

        [Fact(DisplayName = "Установка некорректного ключа - исключение")]
        public async Task SetOrderItemKeyAsync_InvalidKey_ThrowsArgumentException()
        {
            // Arrange
            var orderItemId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var gameId = Guid.NewGuid();
            var invalidKey = "";
            var orderItem = new OrderItem(orderItemId, orderId, gameId, sellerId, null);

            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Seller);
            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId, UserRole.Seller)).ReturnsAsync(orderItem);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _orderService.SetOrderItemKeyAsync(orderItemId, invalidKey, sellerId));
        }

        [Fact(DisplayName = "Получение OrderItems по SellerID - успех")]
        public async Task GetOrderItemsBySellerIdAsync_ReturnsOrderItemDTOList()
        {
            // Arrange
            var sellerId = Guid.NewGuid();
            var orderId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();
            var orderItems = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), orderId, gameId1, sellerId, null),
                new OrderItem(Guid.NewGuid(), orderId, gameId2, sellerId, "KEY-456")
            };

            _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Seller);
            _orderItemRepositoryMock.Setup(repo => repo.GetBySellerIdAsync(sellerId, UserRole.Seller)).ReturnsAsync(orderItems);

            // Act
            var result = await _orderService.GetOrderItemsBySellerIdAsync(sellerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("KEY-456", result[1].Key);
        }

        //[Fact(DisplayName = "Получение OrderItems по SellerID - пустой список")]
        //public async Task GetOrderItemsBySellerIdAsync_NoOrderItems_ReturnsEmptyList()
        //{
        //    // Arrange
        //    var sellerId = Guid.NewGuid();

        //    _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Seller);
        //    _orderItemRepositoryMock.Setup(repo => repo.GetBySellerIdAsync(sellerId, UserRole.Seller)).ReturnsAsync((List<OrderItem>)null); // Имитация null результата

        //    // Act
        //    var result = await _orderService.GetOrderItemsBySellerIdAsync(sellerId);

        //    // Assert
        //    Assert.NotNull(result);
        //    Assert.Empty(result);
        //}
    }
}