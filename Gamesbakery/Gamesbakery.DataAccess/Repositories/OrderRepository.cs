using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly GamesbakeryDbContext _context;

        public OrderRepository(GamesbakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    return await _context.Orders
                        .Where(o => o.UserId == userId)
                        .ToListAsync();
                }
                else
                {
                    return await _context.UserOrders
                        .Where(o => o.UserId == userId)
                        .ToListAsync();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve orders for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<Order> AddAsync(Order order, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    _context.Orders.Add(order);
                    await _context.SaveChangesAsync();
                    return order;
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO UserOrders (OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue) " +
                        "VALUES (@OrderID, @UserID, @OrderDate, @TotalPrice, @IsCompleted, @IsOverdue)",
                        new SqlParameter("@OrderID", order.Id),
                        new SqlParameter("@UserID", order.UserId),
                        new SqlParameter("@OrderDate", order.OrderDate),
                        new SqlParameter("@TotalPrice", order.Price),
                        new SqlParameter("@IsCompleted", order.IsCompleted),
                        new SqlParameter("@IsOverdue", order.IsOverdue));

                    return order;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create order: {ex.Message}", ex);
            }
        }

        public async Task<Order> GetByIdAsync(Guid orderId, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    return await _context.Orders
                        .FirstOrDefaultAsync(o => o.Id == orderId);
                }
                else
                {
                    return await _context.UserOrders
                        .FirstOrDefaultAsync(o => o.Id == orderId);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve order with ID {orderId}: {ex.Message}", ex);
            }
        }

        public async Task<Order> UpdateAsync(Order order, UserRole role)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            try
            {
                if (role == UserRole.Admin)
                {
                    var existingOrder = await _context.Orders.FindAsync(order.Id);
                    if (existingOrder == null)
                        throw new KeyNotFoundException($"Order with ID {order.Id} not found.");

                    existingOrder.SetOrderDate(order.OrderDate);
                    existingOrder.SetPrice(order.Price);
                    existingOrder.SetComplete(order.IsCompleted);
                    existingOrder.SetComplete(order.IsOverdue);
                    await _context.SaveChangesAsync();
                    return existingOrder;
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE UserOrders SET OrderDate = @OrderDate, TotalPrice = @TotalPrice, " +
                        "IsCompleted = @IsCompleted, IsOverdue = @IsOverdue WHERE OrderID = @OrderID",
                        new SqlParameter("@OrderDate", order.OrderDate),
                        new SqlParameter("@TotalPrice", order.Price),
                        new SqlParameter("@IsCompleted", order.IsCompleted),
                        new SqlParameter("@IsOverdue", order.IsOverdue),
                        new SqlParameter("@OrderID", order.Id));

                    return await _context.Orders.FindAsync(order.Id);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update order in the database: {ex.Message}", ex);
            }
        }
    }
}