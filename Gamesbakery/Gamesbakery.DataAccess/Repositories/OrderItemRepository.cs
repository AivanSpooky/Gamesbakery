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
                if (role == UserRole.Admin)
                {
                    await _context.OrderItems.AddAsync(orderItem);
                    await _context.SaveChangesAsync();
                    return orderItem;
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO UserOrderItems (OrderItemID, OrderID, GameID, SellerID, GameKey) " +
                        "VALUES (@OrderItemID, @OrderID, @GameID, @SellerID, @GameKey)",
                        new SqlParameter("@OrderItemID", orderItem.Id),
                        new SqlParameter("@OrderID", orderItem.OrderId),
                        new SqlParameter("@GameID", orderItem.GameId),
                        new SqlParameter("@SellerID", orderItem.SellerId),
                        new SqlParameter("@GameKey", (object)orderItem.Key ?? DBNull.Value));

                    return orderItem;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add order item to the database.", ex);
            }
        }

        public async Task<OrderItem> GetByIdAsync(Guid id, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    var orderItem = await _context.OrderItems.FindAsync(id);
                    if (orderItem == null)
                        throw new KeyNotFoundException($"OrderItem with ID {id} not found.");
                    return orderItem;
                }
                else if (role == UserRole.Seller)
                {
                    var orderItem = await _context.SellerOrderItems
                        .FirstOrDefaultAsync(oi => oi.Id == id);

                    if (orderItem == null)
                        throw new KeyNotFoundException($"OrderItem with ID {id} not found or you do not have access to this order item.");

                    return orderItem;
                }
                else
                {
                    var orderItem = await _context.UserOrderItems
                        .FirstOrDefaultAsync(oi => oi.Id == id);
                    if (orderItem == null)
                        throw new KeyNotFoundException($"OrderItem with ID {id} not found.");
                    return orderItem;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve order item with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    return await _context.OrderItems
                        .Where(oi => oi.OrderId == orderId)
                        .ToListAsync();
                }
                else
                {
                    return await _context.UserOrderItems
                        .Where(oi => oi.OrderId == orderId)
                        .ToListAsync();
                }
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
                if (role == UserRole.Admin || role == UserRole.Seller)
                {
                    return await _context.OrderItems
                        .Where(oi => oi.SellerId == sellerId)
                        .ToListAsync();
                }
                else
                {
                    return await _context.UserOrderItems
                        .Where(oi => oi.SellerId == sellerId)
                        .ToListAsync();
                }
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
                if (role == UserRole.Admin)
                {
                    var existingOrderItem = await _context.OrderItems.FindAsync(orderItem.Id);
                    if (existingOrderItem == null)
                        throw new KeyNotFoundException($"OrderItem with ID {orderItem.Id} not found.");

                    existingOrderItem.SetKey(orderItem.Key);
                    await _context.SaveChangesAsync();
                }
                else if (role == UserRole.Seller)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE SellerOrderItems SET KeyText = @Key WHERE OrderItemID = @OrderItemID AND SellerID = @SellerID",
                        new SqlParameter("@Key", orderItem.Key ?? (object)DBNull.Value),
                        new SqlParameter("@OrderItemID", orderItem.Id),
                        new SqlParameter("@SellerID", orderItem.SellerId));

                    var updatedItem = await _context.OrderItems.FindAsync(orderItem.Id);
                    if (updatedItem?.Key != orderItem.Key)
                        throw new InvalidOperationException("Failed to update order item through view.");
                }
                else
                {
                    throw new UnauthorizedAccessException("Only administrators and sellers can update order items.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update order item in the database: {ex.Message}", ex);
            }
        }
    }
}