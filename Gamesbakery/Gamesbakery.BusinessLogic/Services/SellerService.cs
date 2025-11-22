using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.BusinessLogic.Services
{
    public class SellerService : ISellerService
    {
        private readonly ISellerRepository _sellerRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IOrderService _orderService;

        public SellerService(
            ISellerRepository sellerRepository,
            IOrderItemRepository orderItemRepository,
            IOrderService orderService)
        {
            _sellerRepository = sellerRepository;
            _orderItemRepository = orderItemRepository;
            _orderService = orderService;
        }

        public async Task<SellerDTO> RegisterSellerAsync(string sellerName, string password, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can register sellers");
            var sellerDto = new SellerDTO
            {
                Id = Guid.NewGuid(),
                SellerName = sellerName,
                RegistrationDate = DateTime.UtcNow,
                AvgRating = 0.0,
                Password = password
            };
            return await _sellerRepository.AddAsync(sellerDto, role);
        }

        public async Task<SellerDTO> GetSellerByIdAsync(Guid id, Guid? curSellerId, UserRole role)
        {
            if (role != UserRole.Admin && curSellerId != id)
                throw new UnauthorizedAccessException("Can only view own profile");
            var seller = await _sellerRepository.GetByIdAsync(id, role);
            if (seller == null)
                throw new KeyNotFoundException($"Seller {id} not found");
            return seller;
        }

        public async Task<List<SellerDTO>> GetAllSellersAsync(UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can view all sellers");
            return (await _sellerRepository.GetAllAsync(role)).ToList();
        }

        public async Task UpdateSellerRatingAsync(Guid sellerId, double newRating, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can update ratings");
            var seller = await _sellerRepository.GetByIdAsync(sellerId, role);
            if (seller == null)
                throw new KeyNotFoundException($"Seller {sellerId} not found");
            seller.AvgRating = newRating;
            await _sellerRepository.UpdateAsync(seller, role);
        }

        public async Task<OrderItemDTO> CreateKeyAsync(Guid gameId, string key, Guid? curSellerId, UserRole role)
        {
            if (!curSellerId.HasValue || role != UserRole.Seller)
                throw new UnauthorizedAccessException("Only sellers can create keys");
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be empty");
            var orderItemDto = new OrderItemDTO
            {
                Id = Guid.NewGuid(),
                OrderId = null,
                GameId = gameId,
                SellerId = curSellerId.Value,
                Key = key
            };
            return await _orderItemRepository.AddAsync(orderItemDto, role);
        }

        public async Task<List<OrderItemDTO>> GetOrderItemsBySellerIdAsync(Guid sellerId, Guid? curSellerId, UserRole role)
        {
            return await _orderService.GetOrderItemsBySellerIdAsync(sellerId, curSellerId, role);
        }

        public async Task SetOrderItemKeyAsync(Guid orderItemId, string key, Guid sellerId, Guid? curSellerId, UserRole role)
        {
            await _orderService.SetOrderItemKeyAsync(orderItemId, key, sellerId, curSellerId, role);
        }
    }
}