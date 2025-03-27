using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order order);
        Task<Order> GetByIdAsync(Guid id);
        Task<List<Order>> GetByUserIdAsync(Guid userId);
        Task<Order> UpdateAsync(Order order);
    }
}