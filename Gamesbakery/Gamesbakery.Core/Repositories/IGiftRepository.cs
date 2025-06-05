using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IGiftRepository
    {
        Task<Gift> AddAsync(Gift gift);
        Task<Gift> GetByIdAsync(Guid giftId, UserRole role, GiftSource source, Guid? currentUserId);
        Task<IEnumerable<SentGift>> GetBySenderIdAsync(Guid senderId, UserRole role);
        Task<IEnumerable<ReceivedGift>> GetByRecipientIdAsync(Guid recipientId, UserRole role);
        Task DeleteAsync(Guid giftId);
    }
}
