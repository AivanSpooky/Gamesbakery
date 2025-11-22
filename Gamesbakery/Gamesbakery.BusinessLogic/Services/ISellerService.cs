using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface ISellerService
    {
        Task<SellerDTO> RegisterSellerAsync(string sellerName, string password, UserRole role);
        Task<SellerDTO> GetSellerByIdAsync(Guid id, Guid? curSellerId, UserRole role);
        Task<OrderItemDTO> CreateKeyAsync(Guid gameId, string key, Guid? curSellerId, UserRole role);
        Task<List<SellerDTO>> GetAllSellersAsync(UserRole role);
        Task UpdateSellerRatingAsync(Guid sellerId, double newRating, UserRole role);
        Task<List<OrderItemDTO>> GetOrderItemsBySellerIdAsync(Guid sellerId, Guid? curSellerId, UserRole role);
        Task SetOrderItemKeyAsync(Guid orderItemId, string key, Guid sellerId, Guid? curSellerId, UserRole role);
    }
}
