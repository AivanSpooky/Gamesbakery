using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.CartDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface ICartRepository
    {
        Task<CarTDTO> AddAsync(CarTDTO dto, UserRole role);
        Task DeleteAsync(Guid id, UserRole role);
        Task<IEnumerable<CarTDTO>> GetAllAsync(UserRole role);
        Task RemoveCartItemsAsync(Guid cartId, List<Guid> orderItemIds, UserRole role);
        Task<CarTDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null);
        Task<CarTDTO> UpdateAsync(CarTDTO dto, UserRole role);
        Task<CarTDTO?> GetByUserIdAsync(Guid userId, UserRole role);
        Task AddItemAsync(Guid cartId, Guid orderItemId, UserRole role);
        Task RemoveItemAsync(Guid cartId, Guid orderItemId, UserRole role);
        Task ClearAsync(Guid cartId, UserRole role);
        Task<List<CartItemDTO>> GetItemsAsync(Guid userId, UserRole role);
    }
}