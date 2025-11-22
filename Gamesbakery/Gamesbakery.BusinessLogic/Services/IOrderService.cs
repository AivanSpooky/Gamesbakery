using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.DTOs.OrderItemDTO;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IOrderService
    {
        Task<OrderListDTO> CreateOrderAsync(Guid userId, List<Guid> orderItemIds, Guid? curUserId, UserRole role);
        Task<OrderDetailsDTO> GetOrderByIdAsync(Guid orderId, Guid? curUserId, UserRole role);
        Task<List<OrderListDTO>> GetOrdersByUserIdAsync(Guid userId, UserRole role);
        Task SetOrderItemKeyAsync(Guid orderItemId, string key, Guid sellerId, Guid? curSellerId, UserRole role);
        Task<List<OrderItemDTO>> GetOrderItemsBySellerIdAsync(Guid sellerId, Guid? curSellerId, UserRole role);
    }
}