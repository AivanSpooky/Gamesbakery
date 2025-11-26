using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.BusinessLogic.Services
{
    public class OrderItemService : IOrderItemService
    {
        private readonly IOrderItemRepository _orderItemRepository;

        public OrderItemService(IOrderItemRepository orderItemRepository)
        {
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
        }

        public async Task<OrderItemDTO> CreateAsync(OrderItemCreateDTO dto, Guid? curSellerId, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller)
                throw new UnauthorizedAccessException("Only admins or sellers can create order items.");
            var sellerId = role == UserRole.Seller ? curSellerId ?? throw new UnauthorizedAccessException("Seller ID not found.") : dto.SellerId;
            var orderItemDTO = new OrderItemDTO
            {
                Id = Guid.NewGuid(),
                OrderId = null,
                GameId = dto.GameId,
                SellerId = sellerId,
                Key = dto.Key ?? string.Empty
            };
            return await _orderItemRepository.AddAsync(orderItemDTO, role);
        }

        public async Task<OrderItemDTO> GetByIdAsync(Guid id, Guid? curUserId, UserRole role)
        {
            var orderItem = await _orderItemRepository.GetByIdAsync(id, role, curUserId);
            if (orderItem == null)
                throw new KeyNotFoundException($"OrderItem with ID {id} not found.");
            return orderItem;
        }

        public async Task<List<OrderItemDTO>> GetFilteredAsync(Guid? sellerId, Guid? gameId, Guid? curSellerId, UserRole role)
        {
            if (gameId.HasValue)
                return await _orderItemRepository.GetAvailableByGameIdAsync(gameId.Value, role);
            //if (role != UserRole.Admin && role != UserRole.Seller)
            //    throw new UnauthorizedAccessException("Only admins or sellers can view order items by seller.");
            //if (role == UserRole.Seller && sellerId.HasValue && sellerId != curSellerId)
            //    throw new UnauthorizedAccessException("Sellers can only access their own items.");
            return await _orderItemRepository.GetFilteredAsync(sellerId, gameId, role);
        }

        public async Task UpdateAsync(Guid id, OrderItemUpdateDTO dto, Guid? curSellerId, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller)
                throw new UnauthorizedAccessException("Only admins or sellers can update order items.");
            var orderItem = await _orderItemRepository.GetByIdAsync(id, role, curSellerId);
            if (orderItem == null)
                throw new KeyNotFoundException($"OrderItem with ID {id} not found.");
            if (role == UserRole.Seller && orderItem.SellerId != curSellerId)
                throw new UnauthorizedAccessException("Sellers can only update their own order items.");
            if (!string.IsNullOrEmpty(dto.Key))
                orderItem.Key = dto.Key;
            await _orderItemRepository.UpdateAsync(orderItem, role);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete order items.");
            await _orderItemRepository.DeleteAsync(id, role);
        }
    }
}
