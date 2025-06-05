using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gamesbakery.BusinessLogic.Services
{
    public class GiftService : IGiftService
    {
        private readonly IGiftRepository _giftRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IAuthenticationService _authService;

        public GiftService(
            IGiftRepository giftRepository,
            IOrderItemRepository orderItemRepository,
            IGameRepository gameRepository,
            IAuthenticationService authService)
        {
            _giftRepository = giftRepository ?? throw new ArgumentNullException(nameof(giftRepository));
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        public async Task<IEnumerable<GiftDTO>> GetGiftsBySenderAsync(Guid senderId, UserRole role)
        {
            Guid? curUserId = _authService.GetCurrentUserId();
            if (senderId == Guid.Empty)
                throw new ArgumentException("SenderId cannot be empty.", nameof(senderId));

            var gifts = await _giftRepository.GetBySenderIdAsync(senderId, role);
            var giftDTOs = new List<GiftDTO>();

            foreach (var gift in gifts)
            {
                var orderItem = await _orderItemRepository.GetByIdAsync(gift.OrderItemId, role, curUserId);
                var game = orderItem != null ? await _gameRepository.GetByIdAsync(orderItem.GameId, role) : null;
                giftDTOs.Add(new GiftDTO
                {
                    Id = gift.Id,
                    SenderId = gift.SenderId,
                    RecipientId = gift.RecipientId,
                    GameId = orderItem?.GameId ?? Guid.Empty,
                    GameTitle = game?.Title ?? "Unknown",
                    Key = orderItem?.Key,
                    GiftDate = gift.GiftDate
                });
            }

            return giftDTOs;
        }

        public async Task<IEnumerable<GiftDTO>> GetGiftsByRecipientAsync(Guid recipientId, UserRole role)
        {
            Guid? curUserId = _authService.GetCurrentUserId();
            if (recipientId == Guid.Empty)
                throw new ArgumentException("RecipientId cannot be empty.", nameof(recipientId));

            var gifts = await _giftRepository.GetByRecipientIdAsync(recipientId, role);
            var giftDTOs = new List<GiftDTO>();

            foreach (var gift in gifts)
            {
                var orderItem = await _orderItemRepository.GetByIdAsync(gift.OrderItemId, role, curUserId);
                var game = orderItem != null ? await _gameRepository.GetByIdAsync(orderItem.GameId, role) : null;
                giftDTOs.Add(new GiftDTO
                {
                    Id = gift.Id,
                    SenderId = gift.SenderId,
                    RecipientId = gift.RecipientId,
                    GameId = orderItem?.GameId ?? Guid.Empty,
                    GameTitle = game?.Title ?? "Unknown",
                    Key = orderItem?.Key,
                    GiftDate = gift.GiftDate
                });
            }

            return giftDTOs;
        }

        public async Task<GiftDTO> GetGiftByIdAsync(Guid giftId, UserRole role, GiftSource source)
        {
            if (giftId == Guid.Empty)
                throw new ArgumentException("GiftId cannot be empty.", nameof(giftId));

            var currentUserId = _authService.GetCurrentUserId();
            if (currentUserId == null && role != UserRole.Admin)
                throw new UnauthorizedAccessException("User not authenticated.");

            try
            {
                var gift = await _giftRepository.GetByIdAsync(giftId, role, source, currentUserId);
                if (gift == null)
                    throw new KeyNotFoundException($"Gift with ID {giftId} not found.");

                var orderItem = await _orderItemRepository.GetByIdAsync(gift.OrderItemId, role, currentUserId);
                var game = orderItem != null ? await _gameRepository.GetByIdAsync(orderItem.GameId, role) : null;

                var giftDTO = new GiftDTO
                {
                    Id = gift.Id,
                    SenderId = gift.SenderId,
                    RecipientId = gift.RecipientId,
                    GameId = orderItem?.GameId ?? Guid.Empty,
                    GameTitle = game?.Title ?? "Unknown",
                    Key = orderItem?.Key,
                    GiftDate = gift.GiftDate
                };
                return giftDTO;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task CreateGiftAsync(Guid senderId, Guid recipientId, Guid orderItemId, UserRole role)
        {
            Guid? curUserId = _authService.GetCurrentUserId();
            if (senderId == Guid.Empty || recipientId == Guid.Empty || orderItemId == Guid.Empty)
                throw new ArgumentException("Invalid sender, recipient, or order item ID.");

            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, role, curUserId);
            if (orderItem == null)
                throw new KeyNotFoundException($"OrderItem with ID {orderItemId} not found.");

            if (orderItem.IsGifted)
                throw new InvalidOperationException("This order item has already been gifted.");

            orderItem.SetGifted(true);
            await _orderItemRepository.UpdateAsync(orderItem, UserRole.Admin);

            var gift = new Gift(Guid.NewGuid(), senderId, recipientId, orderItemId, DateTime.UtcNow);
            await _giftRepository.AddAsync(gift);
        }

        public async Task DeleteGiftAsync(Guid giftId, UserRole role)
        {
            if (giftId == Guid.Empty)
                throw new ArgumentException("GiftId cannot be empty.", nameof(giftId));

            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can delete gifts.");

            var gift = await _giftRepository.GetByIdAsync(giftId, role, GiftSource.All, _authService.GetCurrentUserId());
            if (gift == null)
                throw new KeyNotFoundException($"Gift with ID {giftId} not found.");

            await _giftRepository.DeleteAsync(giftId);
        }

        public async Task<IEnumerable<OrderItemGDTO>> GetAvailableOrderItemsAsync(Guid userId, UserRole role)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));

            var orderItems = await _orderItemRepository.GetByUserIdAsync(userId, role);
            var availableOrderItems = orderItems
                .Where(oi => !oi.IsGifted)
                .ToList();

            var orderItemDTOs = new List<OrderItemGDTO>();
            foreach (var orderItem in availableOrderItems)
            {
                var game = await _gameRepository.GetByIdAsync(orderItem.GameId, role);
                orderItemDTOs.Add(new OrderItemGDTO
                {
                    Id = orderItem.Id,
                    GameTitle = game?.Title ?? "Unknown"
                });
            }

            return orderItemDTOs;
        }
    }
}