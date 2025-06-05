using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Gamesbakery.DataAccess.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly GamesbakeryDbContext _context;

        public OrderRepository(GamesbakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            //_logger = Log.ForContext<OrderRepository>();
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId, UserRole role)
        {
            try
            {
                //_logger.Information("Retrieving orders for UserId: {UserId} with Role: {Role}", userId, role);
                string query = role == UserRole.Admin && userId == Guid.Empty
                    ? "SELECT OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue FROM Orders"
                    : "SELECT OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue FROM UserOrders WHERE UserID = @UserID";

                var parameters = role == UserRole.Admin && userId == Guid.Empty
                    ? Array.Empty<SqlParameter>()
                    : new[] { new SqlParameter("@UserID", userId) };

                var orders = await _context.Orders
                    .FromSqlRaw(query, parameters)
                    .ToListAsync();
                //_logger.Information("Retrieved {Count} orders for UserId: {UserId}", orders.Count, userId);
                return orders;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex, "Failed to retrieve orders for UserId: {UserId}", userId);
                throw new InvalidOperationException($"Failed to retrieve orders for user {userId}: {ex.Message}", ex);
            }
        }

        public async Task<Order> AddAsync(Order order, UserRole role)
        {
            try
            {
                //_logger.Information("Adding order with OrderId: {OrderId} for UserId: {UserId}", order.Id, order.UserId);
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Orders (OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue) " +
                    "VALUES (@OrderID, @UserID, @OrderDate, @TotalPrice, @IsCompleted, @IsOverdue)",
                    new SqlParameter("@OrderID", order.Id),
                    new SqlParameter("@UserID", order.UserId),
                    new SqlParameter("@OrderDate", order.OrderDate),
                    new SqlParameter("@TotalPrice", order.Price),
                    new SqlParameter("@IsCompleted", order.IsCompleted),
                    new SqlParameter("@IsOverdue", order.IsOverdue));
                //_logger.Information("Successfully added order with OrderId: {OrderId}", order.Id);
                return order;
            }
            catch (SqlException ex)
            {
                //_logger.Error(ex, "Database error while adding order with OrderId: {OrderId}", order.Id);
                if (ex.Number == 547) // Foreign key violation
                    throw new InvalidOperationException($"Failed to create order: Invalid UserID {order.UserId}.", ex);
                if (ex.Number == 2627 || ex.Number == 2601) // Unique constraint violation
                    throw new InvalidOperationException($"Failed to create order: OrderID {order.Id} already exists.", ex);
                throw new InvalidOperationException($"Failed to create order: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                //_logger.Error(ex, "Failed to add order with OrderId: {OrderId}", order.Id);
                throw new InvalidOperationException($"Failed to create order: {ex.Message}", ex);
            }
        }

        public async Task<Order> GetByIdAsync(Guid orderId, UserRole role, Guid? currentUserId = null)
        {
            try
            {
                //_logger.Information("Retrieving order with OrderId: {OrderId} for Role: {Role}, CurrentUserId: {CurrentUserId}", orderId, role, currentUserId);
                string query = role == UserRole.Admin
                    ? "SELECT OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue FROM Orders WHERE OrderID = @OrderID"
                    : "SELECT OrderID, UserID, OrderDate, TotalPrice, IsCompleted, IsOverdue FROM Orders WHERE OrderID = @OrderID AND UserID = @UserID";

                var parameters = role == UserRole.Admin
                    ? new[] { new SqlParameter("@OrderID", orderId) }
                    : new[]
                    {
                    new SqlParameter("@OrderID", orderId),
                    new SqlParameter("@UserID", currentUserId ?? throw new ArgumentNullException(nameof(currentUserId), "CurrentUserId is required for non-admin role"))
                    };

                var orders = await _context.Orders
                    .FromSqlRaw(query, parameters)
                    .ToListAsync();
                var order = orders.FirstOrDefault();
                if (order == null)
                {
                    //_logger.Warning("Order with ID {OrderId} not found or inaccessible for Role: {Role}, CurrentUserId: {CurrentUserId}", orderId, role, currentUserId);
                    throw new KeyNotFoundException($"Order with ID {orderId} not found or you do not have access.");
                }
                //_logger.Information("Successfully retrieved order with OrderId: {OrderId}", orderId);
                return order;
            }
            catch (Exception ex)
            {
                //_logger.Error(ex, "Failed to retrieve order with OrderId: {OrderId}", orderId);
                throw new InvalidOperationException($"Failed to retrieve order with ID {orderId}: {ex.Message}", ex);
            }
        }

        public async Task<Order> UpdateAsync(Order order, UserRole role)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            try
            {
                //_logger.Information("Updating order with OrderId: {OrderId}", order.Id);
                string query = role == UserRole.Admin
                    ? "UPDATE Orders SET UserID = @UserID, OrderDate = @OrderDate, TotalPrice = @TotalPrice, " +
                      "IsCompleted = @IsCompleted, IsOverdue = @IsOverdue WHERE OrderID = @OrderID"
                    : "UPDATE Orders SET OrderDate = @OrderDate, TotalPrice = @TotalPrice, " +
                      "IsCompleted = @IsCompleted, IsOverdue = @IsOverdue WHERE OrderID = @OrderID AND UserID = SUSER_SID()";

                SqlParameter[] parameters = role == UserRole.Admin
                    ? new[]
                    {
                    new SqlParameter("@UserID", order.UserId),
                    new SqlParameter("@OrderDate", order.OrderDate),
                    new SqlParameter("@TotalPrice", order.Price),
                    new SqlParameter("@IsCompleted", order.IsCompleted),
                    new SqlParameter("@IsOverdue", order.IsOverdue),
                    new SqlParameter("@OrderID", order.Id)
                    }
                    : new[]
                    {
                    new SqlParameter("@OrderDate", order.OrderDate),
                    new SqlParameter("@TotalPrice", order.Price),
                    new SqlParameter("@IsCompleted", order.IsCompleted),
                    new SqlParameter("@IsOverdue", order.IsOverdue),
                    new SqlParameter("@OrderID", order.Id)
                    };

                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(query, parameters);
                if (rowsAffected == 0)
                {
                    //_logger.Warning("Order with ID {OrderId} not found or inaccessible for Role: {Role}", order.Id, role);
                    throw new KeyNotFoundException($"Order with ID {order.Id} not found or you do not have access.");
                }

                var updatedOrder = await GetByIdAsync(order.Id, role);
                //_logger.Information("Successfully updated order with OrderId: {OrderId}", order.Id);
                return updatedOrder;
            }
            catch (SqlException ex)
            {
                //_logger.Error(ex, "Database error while updating order with OrderId: {OrderId}", order.Id);
                if (ex.Number == 547) // Foreign key violation
                    throw new InvalidOperationException($"Failed to update order: Invalid UserID {order.UserId}.", ex);
                throw new InvalidOperationException($"Failed to update order: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                //_logger.Error(ex, "Failed to update order with OrderId: {OrderId}", order.Id);
                throw new InvalidOperationException($"Failed to update order: {ex.Message}", ex);
            }
        }
    }
}