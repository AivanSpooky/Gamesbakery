using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IOrderRepository
    {
        Task<OrderDetailsDTO> AddAsync(OrderDetailsDTO dto, UserRole role);

        Task DeleteAsync(Guid id, UserRole role);

        Task<IEnumerable<OrderListDTO>> GetAllAsync(UserRole role);

        Task<OrderDetailsDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null);

        Task<OrderDetailsDTO> UpdateAsync(OrderDetailsDTO dto, UserRole role);

        Task<IEnumerable<OrderListDTO>> GetByUserIdAsync(Guid userId, UserRole role);
    }
}
