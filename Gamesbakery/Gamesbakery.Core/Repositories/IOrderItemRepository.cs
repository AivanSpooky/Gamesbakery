using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItem> AddAsync(OrderItem orderItem, UserRole role);
        Task<OrderItem> GetByIdAsync(Guid id, UserRole role);
        Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, UserRole role);
        Task<List<OrderItem>> GetBySellerIdAsync(Guid sellerId, UserRole role);
        Task UpdateAsync(OrderItem orderItem, UserRole role);
    }
}