using Gamesbakery.Core.DTOs;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GiftDTO;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IGiftService
    {
        Task<IEnumerable<GiftDTO>> GetGiftsBySenderAsync(Guid senderId, UserRole role);
        Task<IEnumerable<GiftDTO>> GetGiftsByRecipientAsync(Guid recipientId, UserRole role);
        Task<GiftDTO> GetGiftByIdAsync(Guid giftId, UserRole role, GiftSource source);
        Task CreateGiftAsync(Guid senderId, Guid recipientId, Guid orderItemId, UserRole role);
        Task DeleteGiftAsync(Guid giftId, UserRole role);
        Task<IEnumerable<OrderItemGDTO>> GetAvailableOrderItemsAsync(Guid userId, UserRole role);
    }
}
