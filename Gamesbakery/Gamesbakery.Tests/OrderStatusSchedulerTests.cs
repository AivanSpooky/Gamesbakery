using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Schedulers;
using Moq;
using Xunit;

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
            var order = new Order(1, 1, DateTime.UtcNow.AddDays(-5), 100, false, false);
            var orders = new List<Order> { order };
            var orderItems = new List<OrderItem>
            {
                new OrderItem(1, 1, 1, 1, "KEY-123"),
                new OrderItem(2, 1, 2, 1, "KEY-456")
            };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(0)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(1)).ReturnsAsync(orderItems);
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
            var order = new Order(1, 1, DateTime.UtcNow.AddDays(-15), 100, false, false);
            var orders = new List<Order> { order };
            var orderItems = new List<OrderItem>
            {
                new OrderItem(1, 1, 1, 1, null),
                new OrderItem(2, 1, 2, 1, "KEY-456")
            };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(0)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(1)).ReturnsAsync(orderItems);
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
            var order = new Order(1, 1, DateTime.UtcNow.AddDays(-5), 100, true, false);
            var orders = new List<Order> { order };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(0)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(It.IsAny<int>())).ReturnsAsync(new List<OrderItem>());

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
            var order = new Order(1, 1, DateTime.UtcNow.AddDays(-15), 100, false, true);
            var orders = new List<Order> { order };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(0)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(It.IsAny<int>())).ReturnsAsync(new List<OrderItem>());

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
            var order = new Order(1, 1, DateTime.UtcNow.AddDays(-5), 100, false, false);
            var orders = new List<Order> { order };
            var orderItems = new List<OrderItem>
            {
                new OrderItem(1, 1, 1, 1, null),
                new OrderItem(2, 1, 2, 1, "KEY-456")
            };

            _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(0)).ReturnsAsync(orders);
            _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(1)).ReturnsAsync(orderItems);

            // Act
            await _scheduler.UpdateOrderStatusesAsync();

            // Assert
            Assert.False(order.IsCompleted);
            Assert.False(order.IsOverdue);
            _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>()), Times.Never());
        }
    }
}