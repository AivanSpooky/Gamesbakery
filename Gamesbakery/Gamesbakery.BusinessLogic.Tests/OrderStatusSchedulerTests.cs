using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.BusinessLogic.Schedulers;
using Moq;
using Gamesbakery.Core;
using Allure.Xunit.Attributes;
using Allure.Commons;

namespace Gamesbakery.BusinessLogic.Tests
{
    //[Collection("OrderStatusSchedulerCollection")]
    //public class OrderStatusSchedulerTests
    //{
    //    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    //    private readonly Mock<Core.Repositories.IOrderItemRepository> _orderItemRepositoryMock;
    //    private readonly Mock<IAuthenticationService> _authServiceMock;
    //    private readonly OrderStatusScheduler _scheduler;

    //    public OrderStatusSchedulerTests()
    //    {
    //        _orderRepositoryMock = new Mock<IOrderRepository>();
    //        _orderItemRepositoryMock = new Mock<Core.Repositories.IOrderItemRepository>();
    //        _authServiceMock = new Mock<IAuthenticationService>();
    //        _scheduler = new OrderStatusScheduler(_orderRepositoryMock.Object, _orderItemRepositoryMock.Object, _authServiceMock.Object);
    //    }

    //    [AllureSeverity(SeverityLevel.critical)]
    //    [AllureOwner("John Doe")]
    //    [AllureLink("Scheduler Docs", "https://dev.gamesbakery.com/api/scheduler")]
    //    [AllureIssue("SCHED-401")]
    //    [AllureXunit(DisplayName = "Обновление статуса заказа - все ключи сгенерированы, заказ выполнен")]
    //    [Trait("Category", "Unit")]
    //    public async Task UpdateOrderStatusesAsync_AllKeysGenerated_OrderCompleted()
    //    {
    //        var orderId = Guid.NewGuid();
    //        var userId = Guid.NewGuid();
    //        var gameId1 = Guid.NewGuid();
    //        var gameId2 = Guid.NewGuid();
    //        var sellerId = Guid.NewGuid();
    //        var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-5), 100m, "Pending", false, false);
    //        var orders = new List<Order> { order };
    //        var orderItems = new List<OrderItem>
    //        {
    //            new OrderItem(Guid.NewGuid(), orderId, gameId1, sellerId, "KEY-123", false),
    //            new OrderItem(Guid.NewGuid(), orderId, gameId2, sellerId, "KEY-456", false)
    //        };
    //        _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);
    //        _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, UserRole.Admin)).ReturnsAsync(orders);
    //        _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(orderId, UserRole.Admin)).ReturnsAsync(orderItems);
    //        _orderRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Order>(), UserRole.Admin)).ReturnsAsync(order);
    //        await _scheduler.UpdateOrderStatusesAsync();
    //        Assert.True(order.IsCompleted);
    //        Assert.False(order.IsOverdue);
    //    }

    //    [AllureSeverity(SeverityLevel.critical)]
    //    [AllureOwner("Jane Smith")]
    //    [AllureLink("Scheduler Docs", "https://dev.gamesbakery.com/api/scheduler")]
    //    [AllureIssue("SCHED-402")]
    //    [AllureXunit(DisplayName = "Обновление статуса заказа - ключи не сгенерированы, 14 дней прошло, заказ просрочен")]
    //    [Trait("Category", "Unit")]
    //    public async Task UpdateOrderStatusesAsync_KeysNotGenerated_14DaysPassed_OrderOverdue()
    //    {
    //        var orderId = Guid.NewGuid();
    //        var userId = Guid.NewGuid();
    //        var gameId1 = Guid.NewGuid();
    //        var gameId2 = Guid.NewGuid();
    //        var sellerId = Guid.NewGuid();
    //        var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-15), 100m, "Pending", false, false);
    //        var orders = new List<Order> { order };
    //        var orderItems = new List<OrderItem>
    //        {
    //            new OrderItem(Guid.NewGuid(), orderId, gameId1, sellerId, null, false),
    //            new OrderItem(Guid.NewGuid(), orderId, gameId2, sellerId, "KEY-456", false)
    //        };
    //        _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);
    //        _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, UserRole.Admin)).ReturnsAsync(orders);
    //        _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(orderId, UserRole.Admin)).ReturnsAsync(orderItems);
    //        _orderRepositoryMock.Setup(repo => repo.UpdateAsync(It.IsAny<Order>(), UserRole.Admin)).ReturnsAsync(order);
    //        await _scheduler.UpdateOrderStatusesAsync();
    //        Assert.False(order.IsCompleted);
    //        Assert.True(order.IsOverdue);
    //    }

    //    [AllureSeverity(SeverityLevel.normal)]
    //    [AllureOwner("John Doe")]
    //    [AllureLink("Scheduler Docs", "https://dev.gamesbakery.com/api/scheduler")]
    //    [AllureIssue("SCHED-403")]
    //    [AllureXunit(DisplayName = "Обновление статуса заказа - заказ уже выполнен, изменений нет")]
    //    [Trait("Category", "Unit")]
    //    public async Task UpdateOrderStatusesAsync_OrderAlreadyCompleted_NoChanges()
    //    {
    //        var orderId = Guid.NewGuid();
    //        var userId = Guid.NewGuid();
    //        var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-5), 100m, "Completed", true, false);
    //        var orders = new List<Order> { order };
    //        _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);
    //        _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, UserRole.Admin)).ReturnsAsync(orders);
    //        _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(It.IsAny<Guid>(), UserRole.Admin)).ReturnsAsync(new List<OrderItem>());
    //        await _scheduler.UpdateOrderStatusesAsync();
    //        Assert.True(order.IsCompleted);
    //        Assert.False(order.IsOverdue);
    //        _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>(), UserRole.Admin), Times.Never());
    //    }

    //    [AllureSeverity(SeverityLevel.normal)]
    //    [AllureOwner("Jane Smith")]
    //    [AllureLink("Scheduler Docs", "https://dev.gamesbakery.com/api/scheduler")]
    //    [AllureIssue("SCHED-404")]
    //    [AllureXunit(DisplayName = "Обновление статуса заказа - заказ уже просрочен, изменений нет")]
    //    [Trait("Category", "Unit")]
    //    public async Task UpdateOrderStatusesAsync_OrderAlreadyOverdue_NoChanges()
    //    {
    //        var orderId = Guid.NewGuid();
    //        var userId = Guid.NewGuid();
    //        var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-15), 100m, "Pending", false, true);
    //        var orders = new List<Order> { order };
    //        _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);
    //        _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, UserRole.Admin)).ReturnsAsync(orders);
    //        _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(It.IsAny<Guid>(), UserRole.Admin)).ReturnsAsync(new List<OrderItem>());
    //        await _scheduler.UpdateOrderStatusesAsync();
    //        Assert.False(order.IsCompleted);
    //        Assert.True(order.IsOverdue);
    //        _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>(), UserRole.Admin), Times.Never());
    //    }

    //    [AllureSeverity(SeverityLevel.normal)]
    //    [AllureOwner("John Doe")]
    //    [AllureLink("Scheduler Docs", "https://dev.gamesbakery.com/api/scheduler")]
    //    [AllureIssue("SCHED-405")]
    //    [AllureXunit(DisplayName = "Обновление статуса заказа - ключи не сгенерированы, 14 дней не прошло, изменений нет")]
    //    [Trait("Category", "Unit")]
    //    public async Task UpdateOrderStatusesAsync_KeysNotGenerated_LessThan14Days_NoChanges()
    //    {
    //        var orderId = Guid.NewGuid();
    //        var userId = Guid.NewGuid();
    //        var gameId1 = Guid.NewGuid();
    //        var gameId2 = Guid.NewGuid();
    //        var sellerId = Guid.NewGuid();
    //        var order = new Order(orderId, userId, DateTime.UtcNow.AddDays(-5), 100m, "Pending", false, false);
    //        var orders = new List<Order> { order };
    //        var orderItems = new List<OrderItem>
    //        {
    //            new OrderItem(Guid.NewGuid(), orderId, gameId1, sellerId, null, false),
    //            new OrderItem(Guid.NewGuid(), orderId, gameId2, sellerId, "KEY-456", false)
    //        };
    //        _authServiceMock.Setup(auth => auth.GetCurrentRole()).Returns(UserRole.Admin);
    //        _orderRepositoryMock.Setup(repo => repo.GetByUserIdAsync(userId, UserRole.Admin)).ReturnsAsync(orders);
    //        _orderItemRepositoryMock.Setup(repo => repo.GetByOrderIdAsync(orderId, UserRole.Admin)).ReturnsAsync(orderItems);
    //        await _scheduler.UpdateOrderStatusesAsync();
    //        Assert.False(order.IsCompleted);
    //        Assert.False(order.IsOverdue);
    //        _orderRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Order>(), UserRole.Admin), Times.Never());
    //    }
    //}
}
