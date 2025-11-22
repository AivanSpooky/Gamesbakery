using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderItemRepository
    {
        Task<OrderItemDTO> AddAsync(OrderItemDTO dto, UserRole role);
        Task<OrderItemDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null);
        Task<List<OrderItemDTO>> GetByOrderIdAsync(Guid orderId, UserRole role);
        Task<List<OrderItemDTO>> GetBySellerIdAsync(Guid sellerId, UserRole role);
        Task<OrderItemDTO> UpdateAsync(OrderItemDTO dto, UserRole role);
        Task DeleteAsync(Guid id, UserRole role);
        Task<int> GetCountAsync(Guid? sellerId = null, Guid? gameId = null, UserRole role = UserRole.Admin);
        Task<List<OrderItemDTO>> GetFilteredAsync(Guid? sellerId = null, Guid? gameId = null, UserRole role = UserRole.Admin);
        Task<List<OrderItemDTO>> GetAvailableByGameIdAsync(Guid gameId, UserRole role);
        Task<List<OrderItemDTO>> GetByUserIdAsync(Guid userId, UserRole role);
        Task<IEnumerable<OrderItemDTO>> GetAllAsync(UserRole role);
    }
}