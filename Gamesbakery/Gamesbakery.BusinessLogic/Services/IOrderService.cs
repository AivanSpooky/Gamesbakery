using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.DTOs;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IOrderService
    {
        Task<OrderListDTO> CreateOrderAsync(int userId, List<int> gameIds);
        Task<OrderListDTO> GetOrderByIdAsync(int id);
        Task<List<OrderListDTO>> GetOrdersByUserIdAsync(int userId);
        Task SetOrderItemKeyAsync(int orderItemId, string key, int sellerId);
        Task<List<OrderItemDTO>> GetOrderItemsBySellerIdAsync(int sellerId);
    }
}