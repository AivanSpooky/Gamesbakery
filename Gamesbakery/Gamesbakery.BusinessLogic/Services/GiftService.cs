using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.BusinessLogic.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _giftRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IGameRepository _gameRepository;

        public GiftService(IGiftRepository giftRepository, IOrderItemRepository orderItemRepository, IGameRepository gameRepository)
        {
            _giftRepository = giftRepository;
            _orderItemRepository = orderItemRepository;
            _gameRepository = gameRepository;
        }

        public async Task<IEnumerable<GiftDTO>> GetGiftsBySenderAsync(Guid senderId, Guid? curUserId, UserRole role)
        {
            return await _giftRepository.GetBySenderIdAsync(senderId, role);
        }

        public async Task<IEnumerable<GiftDTO>> GetGiftsByRecipientAsync(Guid recipientId, Guid? curUserId, UserRole role)
        {
            return await _giftRepository.GetByRecipientIdAsync(recipientId, role);
        }

        public async Task<GiftDTO> GetGiftByIdAsync(Guid giftId, Guid? curUserId, UserRole role)
        {
            if (curUserId == null && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Not authenticated");
            var gift = await _giftRepository.GetByIdAsync(giftId, role, curUserId);
            if (gift == null)
                throw new KeyNotFoundException($"Gift {giftId} not found");
            return gift;
        }

        public async Task<GiftDTO> SendGiftAsync(Guid senderId, Guid recipientId, Guid orderItemId, Guid? curUserId, UserRole role)
        {
            return await CreateGiftAsync(senderId, recipientId, orderItemId, curUserId, role);
        }

        public async Task<GiftDTO> CreateGiftAsync(Guid senderId, Guid recipientId, Guid orderItemId, Guid? curUserId, UserRole role)
        {
            if (curUserId != senderId && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Can only send from own account");
            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, role, curUserId);
            if (orderItem == null || orderItem.IsGifted)
                throw new InvalidOperationException("Invalid or already gifted order item");
            orderItem.IsGifted = true;
            await _orderItemRepository.UpdateAsync(orderItem, UserRole.Admin);
            var game = await _gameRepository.GetByIdAsync(orderItem.GameId, role);
            var giftDTO = new GiftDTO
            {
                GiftId = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = recipientId,
                OrderItemId = orderItemId,
                GiftDate = DateTime.UtcNow,
                Type = GiftSource.Sent,
                GameTitle = game?.Title ?? "Unknown",
                Key = orderItem.Key
            };
            return await _giftRepository.AddAsync(giftDTO, role);
        }

        public async Task DeleteGiftAsync(Guid giftId, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete gifts");
            await _giftRepository.DeleteAsync(giftId, role);
        }

        public async Task<IEnumerable<OrderItemDTO>> GetAvailableOrderItemsAsync(Guid userId, UserRole role)
        {
            var orderItems = await _orderItemRepository.GetByUserIdAsync(userId, role);
            return orderItems.Where(oi => !oi.IsGifted);
        }
    }
}
