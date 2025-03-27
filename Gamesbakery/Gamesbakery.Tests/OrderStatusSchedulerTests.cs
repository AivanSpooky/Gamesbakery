using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Schedulers;
using Moq;

namespace Gamesbakery.Tests
{
    public class OrderStatusSchedulerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IOrderItemRepository> _orderItemRepositoryMock;
        private readonly OrderStatusScheduler _scheduler;

        public OrderStatusSchedulerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderItemRepositoryMock = new Mock<IOrderItemRepository>();
            _scheduler = new OrderStatusScheduler(_orderRepositoryMock.Object, _orderItemRepositoryMock.Object);
        }

        [Fact(DisplayName = "Обновление статуса заказа - все ключи сгенерированы, заказ выполнен")]
        public async Task UpdateOrderStatusesAsync_AllKeysGenerated_OrderCompleted()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-5), 100, false, false);
            var orders = new List<Order> { order };
            var orderItems = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), orderId, gameId1, sellerId, "KEY-123"),
                new OrderItem(Guid.NewGuid(), orderId, gameId2, sellerId, "KEY-456")
            };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(Guid.Empty)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(orderId)).ReturnsAsync(orderItems);
            _orderRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(order);

            // Act
            await _scheduler.UpdateOrderStatusesAsync();

            // Assert
            Assert.True(order.IsCompleted);
            Assert.False(order.IsOverdue);
        }

        [Fact(DisplayName = "Обновление статуса заказа - ключи не сгенерированы, 14 дней прошло, заказ просрочен")]
        public async Task UpdateOrderStatusesAsync_KeysNotGenerated_14DaysPassed_OrderOverdue()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-15), 100, false, false);
            var orders = new List<Order> { order };
            var orderItems = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), orderId, gameId1, sellerId, null),
                new OrderItem(Guid.NewGuid(), orderId, gameId2, sellerId, "KEY-456")
            };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(Guid.Empty)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(orderId)).ReturnsAsync(orderItems);
            _orderRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Order>())).ReturnsAsync(order);

            // Act
            await _scheduler.UpdateOrderStatusesAsync();

            // Assert
            Assert.False(order.IsCompleted);
            Assert.True(order.IsOverdue);
        }

        [Fact(DisplayName = "Обновление статуса заказа - заказ уже выполнен, изменений нет")]
        public async Task UpdateOrderStatusesAsync_OrderAlreadyCompleted_NoChanges()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-5), 100, true, false);
            var orders = new List<Order> { order };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(Guid.Empty)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<OrderItem>());

            // Act
            await _scheduler.UpdateOrderStatusesAsync();

            // Assert
            Assert.True(order.IsCompleted);
            Assert.False(order.IsOverdue);
            _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Never());
        }

        [Fact(DisplayName = "Обновление статуса заказа - заказ уже просрочен, изменений нет")]
        public async Task UpdateOrderStatusesAsync_OrderAlreadyOverdue_NoChanges()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-15), 100, false, true);
            var orders = new List<Order> { order };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(Guid.Empty)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<OrderItem>());

            // Act
            await _scheduler.UpdateOrderStatusesAsync();

            // Assert
            Assert.False(order.IsCompleted);
            Assert.True(order.IsOverdue);
            _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Never());
        }

        [Fact(DisplayName = "Обновление статуса заказа - ключи не сгенерированы, 14 дней не прошло, изменений нет")]
        public async Task UpdateOrderStatusesAsync_KeysNotGenerated_LessThan14Days_NoChanges()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var gameId1 = Guid.NewGuid();
            var gameId2 = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-5), 100, false, false);
            var orders = new List<Order> { order };
            var orderItems = new List<OrderItem>
            {
                new OrderItem(Guid.NewGuid(), orderId, gameId1, sellerId, null),
                new OrderItem(Guid.NewGuid(), orderId, gameId2, sellerId, "KEY-456")
            };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(Guid.Empty)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(orderId)).ReturnsAsync(orderItems);

            // Act
            await _scheduler.UpdateOrderStatusesAsync();

            // Assert
            Assert.False(order.IsCompleted);
            Assert.False(order.IsOverdue);
            _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Never());
        }
    }
}