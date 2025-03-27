using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
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

        public async Task<OrderItem> AddAsync(OrderItem orderItem)
        {
            try
            {
                await _context.OrderItems.AddAsync(orderItem);
                await _context.SaveChangesAsync();
                return orderItem;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add order item to the database.", ex);
            }
        }

        public async Task<OrderItem> GetByIdAsync(Guid id)
        {
            //if (id <= 0)
            //    throw new ArgumentException("Id must be positive.", nameof(id));

            var orderItem = await _context.OrderItems.FindAsync(id);
            if (orderItem == null)
                throw new KeyNotFoundException($"OrderItem with ID {id} not found.");

            return orderItem;
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId)
        {
            //if (orderId <= 0)
            //    throw new ArgumentException("OrderId must be positive.", nameof(orderId));

            try
            {
                return await _context.OrderItems
                    .Where(oi => oi.OrderId == orderId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve order items for order.", ex);
            }
        }

        public async Task<List<OrderItem>> GetBySellerIdAsync(Guid sellerId)
        {
            //if (sellerId <= 0)
            //    throw new ArgumentException("SellerId must be positive.", nameof(sellerId));

            try
            {
                return await _context.OrderItems
                    .Where(oi => oi.SellerId == sellerId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve order items for seller.", ex);
            }
        }

        public async Task UpdateAsync(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            try
            {
                _context.OrderItems.Update(orderItem);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update order item in the database.", ex);
            }
        }
    }
}