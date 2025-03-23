using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> AddAsync(Order order);
        Task<Order> GetByIdAsync(int id);
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task<Order> UpdateAsync(Order order);
    }
}