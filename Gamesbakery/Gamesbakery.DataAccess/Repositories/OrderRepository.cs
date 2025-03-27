using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
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

        public async Task<Order> AddAsync(Order order)
        {
            try
            {
                await _context.Orders.AddAsync(order);
                await _context.SaveChangesAsync();
                return order;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add order to the database.", ex);
            }
        }

        public async Task<Order> GetByIdAsync(Guid id)
        {
            //if (id <= 0)
            //    throw new ArgumentException("Id must be positive.", nameof(id));

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");

            return order;
        }

        public async Task<List<Order>> GetByUserIdAsync(Guid userId)
        {
            //if (userId <= 0)
            //    throw new ArgumentException("UserId must be positive.", nameof(userId));

            try
            {
                return await _context.Orders
                    .Where(o => o.UserId == userId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve orders for user.", ex);
            }
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            try
            {
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                return await _context.Orders.FindAsync(order.Id);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update order in the database.", ex);
            }
        }
    }
}