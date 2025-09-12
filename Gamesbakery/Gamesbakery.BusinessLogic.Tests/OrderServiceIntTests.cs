using Allure.Xunit.Attributes;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.Repositories;
using Gamesbakery.DataAccess.Tests;
using Gamesbakery.DataAccess.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gamesbakery.BusinessLogic.Tests
{
    [Collection(TestCollections.SqlServer)]
    [AllureTag("Integration")]
    public class OrderServiceIT : IClassFixture<SqlServerDbContextFixture>
    {
        private readonly GamesbakeryDbContext _context;
        private readonly OrderService _orderService;

        public OrderServiceIT(SqlServerDbContextFixture fixture)
        {
            _context = fixture.Context;
            var orderRepo = new OrderRepository(_context);
            var orderItemRepo = new OrderItemRepository(_context);
            var userRepo = new UserRepository(_context);
            var gameRepo = new GameRepository(_context);
            var sellerRepo = new SellerRepository(_context);
            var authService = new TestAuthenticationService();
            _orderService = new OrderService(orderRepo, orderItemRepo, userRepo, gameRepo, sellerRepo, authService);
        }

        [AllureXunit(DisplayName = "ЗАКАЗ: СОЗДАНИЕ (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanCreateOrder()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "OrderUser", "order@example.com", DateTime.UtcNow, "United States", "pass123", false, 200);
            var category = new Category(Guid.NewGuid(), "Action", "Action games");
            var game = new Game(Guid.NewGuid(), category.Id, "Order Game", 50m, DateTime.UtcNow, "Desc", true, "Pub");

            _context.Users.Add(user);
            _context.Categories.Add(category);
            _context.Games.Add(game);
            await _context.SaveChangesAsync();

            // Create order items first
            var orderItem = new OrderItem(Guid.NewGuid(), game.Id, 1, 50m);
            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.CreateOrderAsync(user.Id, new List<Guid> { orderItem.Id });

            // Assert
            Assert.NotNull(result);
            var dbOrder = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.UserId == user.Id);

            Assert.NotNull(dbOrder);
            Assert.Equal(50m, dbOrder.TotalPrice);
        }

        [AllureXunit(DisplayName = "ЗАКАЗ: ПОЛУЧЕНИЕ ПО ПОЛЬЗОВАТЕЛЮ (SQL Server)")]
        [Trait("Category", "Integration")]
        public async Task CanGetOrdersByUser()
        {
            // Arrange
            var user = new User(Guid.NewGuid(), "OrderUser2", "order2@example.com", DateTime.UtcNow, "United States", "pass123", false, 200);
            var order = new Order(Guid.NewGuid(), user.Id, DateTime.UtcNow, 50m, false, false);
            _context.Users.Add(user);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Act
            var result = await _orderService.GetOrdersByUserIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }
    }
}