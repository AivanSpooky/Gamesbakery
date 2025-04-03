using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderRepository
    {
        Task<List<Order>> GetByUserIdAsync(Guid userId, UserRole role);
        Task<Order> AddAsync(Order order, UserRole role);
        Task<Order> GetByIdAsync(Guid orderId, UserRole role);
        Task<Order> UpdateAsync(Order order, UserRole role);
    }
}