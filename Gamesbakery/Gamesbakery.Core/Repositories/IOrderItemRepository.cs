using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItem> AddAsync(OrderItem orderItem);
        Task<OrderItem> GetByIdAsync(int id);
        Task<List<OrderItem>> GetByOrderIdAsync(int orderId);
        Task<List<OrderItem>> GetBySellerIdAsync(int sellerId);
        Task UpdateAsync(OrderItem orderItem);
    }
}