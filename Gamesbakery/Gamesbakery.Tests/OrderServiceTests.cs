using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Services;
using Moq;
using Xunit;

namespace Gamesbakery.Tests
{
    public class OrderServiceTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IGameRepository> _gameRepositoryMock;
        private readonly Mock<ISellerRepository> _sellerRepositoryMock;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _gameRepositoryMock = new Mock<IGameRepository>();
            _sellerRepositoryMock = new Mock<ISellerRepository>();
            _orderService = new OrderService(_orderRepositoryMock.Object, _orderItemRepositoryMock.Object, _userRepositoryMock.Object, _gameRepositoryMock.Object, _sellerRepositoryMock.Object);
        }

        [Fact(DisplayName = "Создание заказа с корректными данными - успех")]
        public async Task CreateOrderAsync_ValidData_ReturnsOrderDTO()
        {
            // Arrange
            var userId = 1;
            var gameIds = new List<int> { 1, 2 };
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", false, 200);
            var game1 = new Game(1, 1, "Game 1", 50, DateTime.UtcNow, "Desc 1", true, "Bethesda");
            var game2 = new Game(2, 1, "Game 2", 50, DateTime.UtcNow, "Desc 2", true, "Valve");
            var seller = new Seller(1, "Seller1", DateTime.UtcNow, 4.5);
            var order = new Order(1, userId, DateTime.UtcNow, 100, false, false);

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(game1);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(2)).ReturnsAsync(game2);
            _sellerRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Seller> { seller });
            _orderRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<Order>())).ReturnsAsync(order);
            _orderItemRepositoryMock.Setup(repo => repo.AddAsync(It.IsAny<OrderItem>())).ReturnsAsync((OrderItem orderItem) => orderItem);
            _userRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<User>())).ReturnsAsync(user);

            // Act
            var result = await _orderService.CreateOrderAsync(userId, gameIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Price);
            Assert.False(result.IsCompleted);
        }

        [Fact(DisplayName = "Создание заказа с недостаточным балансом - исключение")]
        public async Task CreateOrderAsync_InsufficientBalance_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var gameIds = new List<int> { 1 };
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", false, 20);
            var game = new Game(1, 1, "Game 1", 50, DateTime.UtcNow, "Desc 1", true, "Bethesda");
            var seller = new Seller(1, "Seller1", DateTime.UtcNow, 4.5); // Создаём продавца

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(game);
            _sellerRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Seller> { seller });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [Fact(DisplayName = "Создание заказа для заблокированного пользователя - исключение")]
        public async Task CreateOrderAsync_BlockedUser_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var gameIds = new List<int> { 1 };
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", true, 200);
            var game = new Game(1, 1, "Game 1", 50, DateTime.UtcNow, "Desc 1", true, "Bethesda");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [Fact(DisplayName = "Создание заказа с несуществующей игрой - исключение")]
        public async Task CreateOrderAsync_GameNotFound_ThrowsKeyNotFoundException()
        {
            // Arrange
            var userId = 1;
            var gameIds = new List<int> { 1 };
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", false, 200);

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync((Game)null);

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [Fact(DisplayName = "Создание заказа с игрой, не доступной для продажи - исключение")]
        public async Task CreateOrderAsync_GameNotForSale_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var gameIds = new List<int> { 1 };
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", false, 200);
            var game = new Game(1, 1, "Game 1", 50, DateTime.UtcNow, "Desc 1", false, "Bethesda");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(game);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [Fact(DisplayName = "Создание заказа без доступных продавцов - исключение")]
        public async Task CreateOrderAsync_NoSellersAvailable_ThrowsInvalidOperationException()
        {
            // Arrange
            var userId = 1;
            var gameIds = new List<int> { 1 };
            var user = new User(userId, "JohnDoe", "john.doe@example.com", DateTime.UtcNow, "USA", "password123", false, 200);
            var game = new Game(1, 1, "Game 1", 50, DateTime.UtcNow, "Desc 1", true, "Bethesda");

            _userRepositoryMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);
            _gameRepositoryMock.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(game);
            _sellerRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Seller>());

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.CreateOrderAsync(userId, gameIds));
        }

        [Fact(DisplayName = "Получение заказов по ID пользователя - успех")]
        public async Task GetOrdersByUserIdAsync_ReturnsOrderDTOList()
        {
            // Arrange
            var userId = 1;
            var orders = new List<Order>
            {
                new Order(1, userId, DateTime.UtcNow, 100, false, false),
                new Order(2, userId, DateTime.UtcNow, 50, true, false)
            };
            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId)).ReturnsAsync(orders);

            // Act
            var result = await _orderService.GetOrdersByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact(DisplayName = "Установка ключа для OrderItem - успех")]
        public async Task SetOrderItemKeyAsync_ValidData_Success()
        {
            // Arrange
            var orderItemId = 1;
            var sellerId = 1;
            var key = "KEY-123";
            var orderItem = new OrderItem(orderItemId, 1, 1, sellerId, null);

            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId)).ReturnsAsync(orderItem);
            _orderItemRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<OrderItem>())).Callback<OrderItem>(item => item.SetKey(key)); // Имитация обновления

            // Act
            await _orderService.SetOrderItemKeyAsync(orderItemId, key, sellerId);

            // Assert
            Assert.Equal(key, orderItem.Key);
            _orderItemRepositoryMock.Verify(repo => repo.UpdateAsync(It.Is<OrderItem>(item => item.Id == orderItemId && item.Key == key)), Times.Once());
        }

        [Fact(DisplayName = "Установка ключа для чужого OrderItem - исключение")]
        public async Task SetOrderItemKeyAsync_WrongSeller_ThrowsInvalidOperationException()
        {
            // Arrange
            var orderItemId = 1;
            var sellerId = 1;
            var wrongSellerId = 2;
            var key = "KEY-123";
            var orderItem = new OrderItem(orderItemId, 1, 1, sellerId, null);

            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId)).ReturnsAsync(orderItem);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _orderService.SetOrderItemKeyAsync(orderItemId, key, wrongSellerId));
        }

        [Fact(DisplayName = "Установка некорректного ключа - исключение")]
        public async Task SetOrderItemKeyAsync_InvalidKey_ThrowsArgumentException()
        {
            // Arrange
            var orderItemId = 1;
            var sellerId = 1;
            var invalidKey = "";
            var orderItem = new OrderItem(orderItemId, 1, 1, sellerId, null);

            _orderItemRepositoryMock.Setup(repo => repo.GetByIdAsync(orderItemId)).ReturnsAsync(orderItem);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _orderService.SetOrderItemKeyAsync(orderItemId, invalidKey, sellerId));
        }

        [Fact(DisplayName = "Получение OrderItems по SellerID - успех")]
        public async Task GetOrderItemsBySellerIdAsync_ReturnsOrderItemDTOList()
        {
            // Arrange
            var sellerId = 1;
            var orderItems = new List<OrderItem>
            {
                new OrderItem(1, 1, 1, sellerId, null),
                new OrderItem(2, 1, 2, sellerId, "KEY-456")
            };

            _orderItemRepositoryMock.Setup(repo => repo.GetBySellerIdAsync(sellerId)).ReturnsAsync(orderItems);

            // Act
            var result = await _orderService.GetOrderItemsBySellerIdAsync(sellerId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("KEY-456", result[1].Key);
        }

        [Fact(DisplayName = "Получение OrderItems по SellerID - пустой список")]
        public async Task GetOrderItemsBySellerIdAsync_NoOrderItems_ReturnsEmptyList()
        {
            // Arrange
            var sellerId = 1;
            _orderItemRepositoryMock.Setup(repo => repo.GetBySellerIdAsync(sellerId)).ReturnsAsync(new List<OrderItem>());

            // Act
            var result = await _orderService.GetOrderItemsBySellerIdAsync(sellerId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}