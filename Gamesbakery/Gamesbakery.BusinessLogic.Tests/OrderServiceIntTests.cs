using Allure.Xunit.Attributes;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess;
using Gamesbakery.DataAccess.Repositories;
using Gamesbakery.DataAccess.Tests;
using Gamesbakery.DataAccess.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamesbakery.BusinessLogic.Tests
{
    //[Collection(TestCollections.SqlServer)]
    //[AllureTag("Integration")]
    //public class OrderServiceIT : IClassFixture<SqlServerDbContextFixture>
    //{
    //    private readonly GamesbakeryDbContext _context;
    //    private readonly OrderService _orderService;

    //    public OrderServiceIT(SqlServerDbContextFixture fixture)
    //    {
    //        _context = fixture.Context;
    //        var orderRepo = new OrderRepository(_context);
    //        var orderItemRepo = new OrderItemRepository(_context);
    //        var userRepo = new UserRepository(_context);
    //        var gameRepo = new GameRepository(_context);
    //        var sellerRepo = new SellerRepository(_context);
    //        var authService = new TestAuthenticationService();
    //        _orderService = new OrderService(orderRepo, orderItemRepo, userRepo, gameRepo, sellerRepo, authService);
    //    }

        //[AllureXunit(DisplayName = "ЗАКАЗ: СОЗДАНИЕ (SQL Server)")]
        //[Trait("Category", "Integration")]
        //public async Task CanCreateOrder()
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Arrange: используем уникальные идентификаторы
        //        var userId = Guid.NewGuid();
        //        var username = $"OrderUser_{Guid.NewGuid():N}[0..8]";
        //        var user = new User(userId, username, $"order{Guid.NewGuid():N}@example.com", DateTime.UtcNow, "United States", "pass123", false, 200);

        //        var categoryId = Guid.NewGuid();
        //        var category = new Category(categoryId, "Action", "Action games");

        //        var gameId = Guid.NewGuid();
        //        var game = new Game(gameId, categoryId, "Order Game", 50m, DateTime.UtcNow, "Desc", true, "Pub");

        //        // ИСПРАВЛЕНО: создаем Seller перед OrderItem
        //        var sellerId = Guid.NewGuid();
        //        var seller = new Seller(sellerId, "RPG Seller", DateTime.UtcNow, 0.0, "1");

        //        // Сохраняем в правильном порядке: User -> Category -> Game -> Seller
        //        _context.Users.Add(user);
        //        _context.Categories.Add(category);
        //        _context.Games.Add(game);
        //        _context.Sellers.Add(seller);
        //        await _context.SaveChangesAsync();

        //        // ИСПРАВЛЕНО: сначала создаем Order, затем OrderItem
        //        var orderId = Guid.NewGuid();
        //        var order = new Order(orderId, userId, DateTime.UtcNow, 50m, false, false);
        //        _context.Orders.Add(order);
        //        await _context.SaveChangesAsync();

        //        var orderItemId = Guid.NewGuid();
        //        var orderItem = new OrderItem(orderItemId, orderId, gameId, sellerId, null, false);
        //        _context.OrderItems.Add(orderItem);
        //        await _context.SaveChangesAsync();

        //        // Act: создаем заказ через сервис
        //        var result = await _orderService.CreateOrderAsync(userId, new List<Guid> { orderItemId });

        //        // Assert
        //        Assert.NotNull(result);
        //        var dbOrder = await _context.Orders
        //            .FirstOrDefaultAsync(o => o.UserId == userId && o.Id == orderId);
        //        Assert.NotNull(dbOrder);
        //        Assert.Equal(50m, dbOrder.Price);
        //        Assert.Equal(userId, dbOrder.UserId);
        //    }
        //    finally
        //    {
        //        await transaction.RollbackAsync();
        //    }
        //}

        //[AllureXunit(DisplayName = "ЗАКАЗ: ПОЛУЧЕНИЕ ПО ПОЛЬЗОВАТЕЛЮ (SQL Server)")]
        //[Trait("Category", "Integration")]
        //public async Task CanGetOrdersByUser()
        //{
        //    await using var transaction = await _context.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Arrange: используем уникальные идентификаторы
        //        var userId = Guid.NewGuid();
        //        var username = $"OrderUser2_{Guid.NewGuid():N}[0..8]";
        //        var user = new User(userId, username, $"order2{Guid.NewGuid():N}@example.com", DateTime.UtcNow, "United States", "pass123", false, 200);

        //        var categoryId = Guid.NewGuid();
        //        var category = new Category(categoryId, "RPG", "RPG games");

        //        var gameId = Guid.NewGuid();
        //        var game = new Game(gameId, categoryId, "RPG Game", 30m, DateTime.UtcNow, "RPG Description", true, "RPG Publisher");

        //        var sellerId = Guid.NewGuid();
        //        var seller = new Seller(sellerId, "RPG Seller", DateTime.UtcNow, 0.0, "1");

        //        // Сохраняем зависимости: User -> Category -> Game -> Seller
        //        _context.Users.Add(user);
        //        _context.Categories.Add(category);
        //        _context.Games.Add(game);
        //        _context.Sellers.Add(seller);
        //        await _context.SaveChangesAsync();

        //        // Создаем заказ
        //        var orderId = Guid.NewGuid();
        //        var order = new Order(orderId, userId, DateTime.UtcNow, 30m, false, false);
        //        _context.Orders.Add(order);
        //        await _context.SaveChangesAsync();

        //        // Создаем OrderItem с правильными ссылками
        //        var orderItemId = Guid.NewGuid();
        //        var orderItem = new OrderItem(orderItemId, orderId, gameId, sellerId, null, false);
        //        _context.OrderItems.Add(orderItem);
        //        await _context.SaveChangesAsync();

        //        // Act
        //        var result = await _orderService.GetOrdersByUserIdAsync(userId);

        //        // Assert
        //        Assert.NotNull(result);
        //        Assert.Single(result);
        //        var retrievedOrder = result.First();
        //        Assert.Equal(orderId, retrievedOrder.Id);
        //        Assert.Equal(30m, retrievedOrder.Price);
        //    }
        //    finally
        //    {
        //        await transaction.RollbackAsync();
        //    }
        //}
    //}
}
