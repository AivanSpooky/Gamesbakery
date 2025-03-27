using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItem> AddAsync(OrderItem orderItem);
        Task<OrderItem> GetByIdAsync(Guid id);
        Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId);
        Task<List<OrderItem>> GetBySellerIdAsync(Guid sellerId);
        Task UpdateAsync(OrderItem orderItem);
    }
}