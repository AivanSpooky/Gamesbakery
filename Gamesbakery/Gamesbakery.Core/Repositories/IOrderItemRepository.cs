using System.Collections.Generic;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItem> AddAsync(OrderItem orderItem, UserRole role);
        Task<OrderItem> CreateKeyAsync(Guid gameId, Guid sellerId, string key, UserRole role);
        Task<OrderItem> GetByIdAsync(Guid id, UserRole role, Guid? currentUserId);
        Task<IEnumerable<OrderItem>> GetByUserIdAsync(Guid userId, UserRole role);
        Task<List<OrderItem>> GetByOrderIdAsync(Guid orderId, UserRole role);
        Task<List<OrderItem>> GetBySellerIdAsync(Guid sellerId, UserRole role);
        Task UpdateAsync(OrderItem orderItem, UserRole role);
        Task<List<OrderItem>> GetAvailableByGameIdAsync(Guid gameId);
    }
}