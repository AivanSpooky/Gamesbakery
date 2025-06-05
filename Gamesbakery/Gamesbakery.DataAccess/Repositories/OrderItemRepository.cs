using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly GamesbakeryDbContext _context;

        public OrderItemRepository(GamesbakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<OrderItem> AddAsync(OrderItem orderItem, UserRole role)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                   "INSERT INTO OrderItems (OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted) " +
                   "VALUES (@OrderItemID, @OrderID, @GameID, @SellerID, @KeyText, @IsGifted)",
                   new SqlParameter("@OrderItemID", orderItem.Id),
                   new SqlParameter("@OrderID", orderItem.OrderId == Guid.Empty ? (object)DBNull.Value : orderItem.OrderId),
                   new SqlParameter("@GameID", orderItem.GameId),
                   new SqlParameter("@SellerID", orderItem.SellerId),
                   new SqlParameter("@KeyText", orderItem.Key ?? (object)DBNull.Value),
                   new SqlParameter("@IsGifted", orderItem.IsGifted));
                return orderItem;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add order item to the database.", ex);
            }
        }

        public async Task<OrderItem> CreateKeyAsync(Guid gameId, Guid sellerId, string key, UserRole role)
        {
            if (role != UserRole.Seller && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only sellers or admins can create keys.");

            var orderItem = new OrderItem(
                Guid.NewGuid(),
                Guid.Empty,
                gameId,
                sellerId,
                key);

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO OrderItems (OrderItemID, GameID, SellerID, KeyText, IsGifted) " +
                    "VALUES (@OrderItemID, @GameID, @SellerID, @KeyText, @IsGifted)",
                    new SqlParameter("@OrderItemID", orderItem.Id),
                    new SqlParameter("@GameID", orderItem.GameId),
                    new SqlParameter("@SellerID", orderItem.SellerId),
                    new SqlParameter("@KeyText", (object)orderItem.Key ?? DBNull.Value),
                    new SqlParameter("@IsGifted", orderItem.IsGifted));
                return orderItem;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to create key for game {gameId} by seller {sellerId}.", ex);
            }
        }


        public async Task<OrderItem> GetByIdAsync(Guid orderItemId, UserRole role, Guid? currentUserId = null)
        {
            try
            {
                string query;
                SqlParameter[] parameters;

                if (role == UserRole.Admin)
                {
                    query = "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE OrderItemID = @OrderItemID";
                    parameters = new[] { new SqlParameter("@OrderItemID", orderItemId) };
                }
                else if (role == UserRole.Seller)
                {
                    if (currentUserId == null)
                        throw new ArgumentNullException(nameof(currentUserId), "CurrentUserId is required for Seller role");
                    query = "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE OrderItemID = @OrderItemID AND SellerID = @SellerID";
                    parameters = new[]
                    {
                    new SqlParameter("@OrderItemID", orderItemId),
                    new SqlParameter("@SellerID", currentUserId.Value)
                };
                }
                else if (role == UserRole.User)
                {
                    if (currentUserId == null)
                        throw new ArgumentNullException(nameof(currentUserId), "CurrentUserId is required for User role");
                    //query = "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems " +
                    //        "WHERE OrderItemID = @OrderItemID AND (OrderID IS NULL OR OrderID IN (SELECT OrderID FROM Orders WHERE UserID = @UserID))";
                    query = "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM UserOrderItems WHERE OrderItemID = @OrderItemID";
                    parameters = new[]
                    {
                    new SqlParameter("@OrderItemID", orderItemId),
                    new SqlParameter("@UserID", currentUserId.Value)
                };
                }
                else
                    throw new UnauthorizedAccessException("Invalid role for accessing order items.");

                var orderItem = await _context.OrderItems
                    .FromSqlRaw(query, parameters)
                    .FirstOrDefaultAsync();

                if (orderItem == null)
                    throw new KeyNotFoundException($"OrderItem with ID {orderItemId} not found or you do not have access to this order item.");

                return orderItem;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve OrderItem with ID {orderItemId}: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<OrderItem>> GetByUserIdAsync(Guid userId, UserRole role)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            try
            {
                return await _context.OrderItems
                    .FromSqlRaw("SELECT oi.OrderItemID, oi.OrderID, oi.GameID, oi.SellerID, oi.KeyText, oi.IsGifted " +
                                "FROM OrderItems oi JOIN Orders o ON oi.OrderID = o.OrderID " +
                                "WHERE o.UserID = @UserID ORDER BY o.OrderDate",
                        new SqlParameter("@UserID", userId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve order items for user {userId}.", ex);
            }
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, UserRole role)
        {
            try
            {
                string query = role == UserRole.Admin
                    ? "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE OrderID = @OrderID"
                    : "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM UserOrderItems WHERE OrderID = @OrderID";

                return await _context.OrderItems
                    .FromSqlRaw(query, new SqlParameter("@OrderID", orderId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve order items for order.", ex);
            }
        }

        public async Task<List<OrderItem>> GetBySellerIdAsync(Guid sellerId, UserRole role)
        {
            try
            {
                string query = (role == UserRole.Admin || role == UserRole.Seller)
                    ? "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM SellerOrderItems WHERE SellerID = @SellerID"
                    : "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM UserOrderItems WHERE SellerID = @SellerID";

                return await _context.OrderItems
                    .FromSqlRaw(query, new SqlParameter("@SellerID", sellerId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve order items for seller.", ex);
            }
        }

        public async Task UpdateAsync(OrderItem orderItem, UserRole role)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            try
            {
                string query;
                SqlParameter[] parameters;

                if (role == UserRole.Admin)
                {
                    query = "UPDATE OrderItems SET OrderID = @OrderID, KeyText = @KeyText, IsGifted = @IsGifted WHERE OrderItemID = @OrderItemID";
                    parameters = new[]
                    {
                    new SqlParameter("@OrderID", (object)orderItem.OrderId ?? DBNull.Value),
                    new SqlParameter("@KeyText", (object)orderItem.Key ?? DBNull.Value),
                    new SqlParameter("@IsGifted", orderItem.IsGifted),
                    new SqlParameter("@OrderItemID", orderItem.Id)
                };
                }
                else if (role == UserRole.Seller)
                {
                    query = "UPDATE OrderItems SET KeyText = @KeyText WHERE OrderItemID = @OrderItemID AND SellerID = @SellerID";
                    parameters = new[]
                    {
                    new SqlParameter("@KeyText", (object)orderItem.Key ?? DBNull.Value),
                    new SqlParameter("@OrderItemID", orderItem.Id),
                    new SqlParameter("@SellerID", orderItem.SellerId)
                };
                }
                else
                    throw new UnauthorizedAccessException("Only administrators and sellers can update order items.");

                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(query, parameters);
                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"OrderItem with ID {orderItem.Id} not found or you do not have access.");
            }
            catch (SqlException ex)
            {
                if (ex.Number == 547) // Foreign key violation
                    throw new InvalidOperationException($"Failed to update OrderItem: Invalid OrderID {orderItem.OrderId} or SellerID {orderItem.SellerId}.", ex);
                throw new InvalidOperationException($"Failed to update OrderItem: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update OrderItem in the database: {ex.Message}", ex);
            }
        }
        public async Task<List<OrderItem>> GetAvailableByGameIdAsync(Guid gameId)
        {
            try
            {
                var result = await _context.OrderItems
                    .FromSqlRaw(
                        "SELECT OrderItemID, OrderID, GameID, SellerID, KeyText, IsGifted FROM OrderItems WHERE GameID = @GameID AND OrderID IS NULL",
                        new SqlParameter("@GameID", gameId))
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve available OrderItems for GameID {gameId}.", ex);
            }
        }
    }
}