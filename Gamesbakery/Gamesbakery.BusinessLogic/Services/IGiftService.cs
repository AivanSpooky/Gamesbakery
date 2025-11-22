using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IGiftService
    {
        Task<IEnumerable<GiftDTO>> GetGiftsBySenderAsync(Guid senderId, Guid? curUserId, UserRole role);
        Task<IEnumerable<GiftDTO>> GetGiftsByRecipientAsync(Guid recipientId, Guid? curUserId, UserRole role);
        Task<GiftDTO> GetGiftByIdAsync(Guid giftId, Guid? curUserId, UserRole role);
        Task<GiftDTO> SendGiftAsync(Guid senderId, Guid recipientId, Guid orderItemId, Guid? curUserId, UserRole role);
        Task<GiftDTO> CreateGiftAsync(Guid senderId, Guid recipientId, Guid orderItemId, Guid? curUserId, UserRole role);
        Task DeleteGiftAsync(Guid giftId, UserRole role);
        Task<IEnumerable<OrderItemDTO>> GetAvailableOrderItemsAsync(Guid userId, UserRole role);
    }
}