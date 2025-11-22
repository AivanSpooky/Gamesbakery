// File: Gamesbakery.Core\IOrderItemService.cs
using System;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core
{
    public interface IOrderItemService
    {
        Task<OrderItemDTO> CreateAsync(OrderItemCreateDTO dto, Guid? curSellerId, UserRole role);
        Task<OrderItemDTO> GetByIdAsync(Guid id, Guid? curUserId, UserRole role);
        Task<List<OrderItemDTO>> GetFilteredAsync(Guid? sellerId, Guid? gameId, Guid? curSellerId, UserRole role);
        Task UpdateAsync(Guid id, OrderItemUpdateDTO dto, Guid? curSellerId, UserRole role);
        Task DeleteAsync(Guid id, UserRole role);
    }
}