using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.DTOs;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IOrderService
    {
        Task<OrderListDTO> CreateOrderAsync(Guid userId, List<Guid> gameIds);
        Task<OrderListDTO> GetOrderByIdAsync(Guid id);
        Task<List<OrderListDTO>> GetOrdersByUserIdAsync(Guid userId);
        Task SetOrderItemKeyAsync(Guid orderItemId, string key, Guid sellerId);
        Task<List<OrderItemDTO>> GetOrderItemsBySellerIdAsync(Guid sellerId);
    }
}