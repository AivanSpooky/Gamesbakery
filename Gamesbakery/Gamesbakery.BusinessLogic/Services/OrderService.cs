using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.BusinessLogic.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IGameRepository _gameRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IOrderItemRepository orderItemRepository,
            IUserRepository userRepository,
            ICartRepository cartRepository,
            IGameRepository gameRepository)
        {
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _userRepository = userRepository;
            _cartRepository = cartRepository;
            _gameRepository = gameRepository;
        }

        public async Task<OrderListDTO> CreateOrderAsync(Guid userId, List<Guid> orderItemIds, Guid? curUserId, UserRole role)
        {
            if (role != UserRole.Admin && userId != curUserId)
                throw new UnauthorizedAccessException("Can only create orders for yourself");
            var cart = await _cartRepository.GetByUserIdAsync(userId, role);
            if (cart == null)
                throw new InvalidOperationException("Cart not found");
            var cartOrderItemIds = cart.Items.Select(ci => ci.OrderItemId).ToList();
            List<Guid> itemsToProcess = orderItemIds == null || !orderItemIds.Any() ? cartOrderItemIds : orderItemIds;
            if (itemsToProcess.Except(cartOrderItemIds).Any())
                throw new InvalidOperationException($"Some items not found in cart");
            if (!itemsToProcess.Any())
                throw new InvalidOperationException("No valid items to process");
            var user = await _userRepository.GetByIdAsync(userId, role);
            if (user == null || user.IsBlocked)
                throw new InvalidOperationException("Invalid or blocked user");
            decimal totalPrice = 0;
            var validItems = new List<OrderItemDTO>();
            foreach (var orderItemId in itemsToProcess)
            {
                var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, UserRole.Admin, null);
                if (orderItem?.OrderId != null)
                    throw new InvalidOperationException($"OrderItem {orderItemId} already in order");
                if (orderItem?.IsGifted == true)
                    throw new InvalidOperationException($"OrderItem {orderItemId} already gifted");
                var game = await _gameRepository.GetByIdAsync(orderItem.GameId, role);
                if (game?.IsForSale != true)
                    throw new InvalidOperationException($"Game {orderItem.GameId} not for sale");
                totalPrice += game.Price;
                validItems.Add(orderItem);
            }
            if (user.Balance < totalPrice)
                throw new InvalidOperationException($"Insufficient balance: {totalPrice} > {user.Balance}");
            var orderDto = new OrderDetailsDTO
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalPrice = totalPrice,
                IsCompleted = false,
                IsOverdue = false,
                OrderItems = new List<OrderItemDTO>()
            };
            var createdOrder = await _orderRepository.AddAsync(orderDto, role);
            foreach (var item in validItems)
            {
                item.OrderId = createdOrder.Id;
                await _orderItemRepository.UpdateAsync(item, UserRole.Admin);
            }
            user.Balance -= totalPrice;
            await _userRepository.UpdateAsync(user, role);
            await _cartRepository.RemoveCartItemsAsync(cart.CartId, itemsToProcess, role);
            return new OrderListDTO
            {
                OrderId = createdOrder.Id,
                OrderDate = createdOrder.OrderDate,
                TotalAmount = createdOrder.TotalPrice,
                IsCompleted = createdOrder.IsCompleted,
                IsOverdue = createdOrder.IsOverdue
            };
        }

        public async Task<OrderDetailsDTO> GetOrderByIdAsync(Guid orderId, Guid? curUserId, UserRole role)
        {
            var order = await _orderRepository.GetByIdAsync(orderId, role, curUserId);
            if (order == null)
                throw new KeyNotFoundException($"Order {orderId} not found");
            return order;
        }

        public async Task<List<OrderListDTO>> GetOrdersByUserIdAsync(Guid userId, UserRole role)
        {
            return (await _orderRepository.GetByUserIdAsync(userId, role)).ToList();
        }

        public async Task SetOrderItemKeyAsync(Guid orderItemId, string key, Guid sellerId, Guid? curSellerId, UserRole role)
        {
            if (role != UserRole.Admin && curSellerId != sellerId)
                throw new UnauthorizedAccessException("Can only set keys for own items");
            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, role, curSellerId);
            if (orderItem?.SellerId != sellerId)
                throw new InvalidOperationException("Cannot set key for this order item");
            orderItem.Key = key;
            await _orderItemRepository.UpdateAsync(orderItem, role);
        }

        public async Task<List<OrderItemDTO>> GetOrderItemsBySellerIdAsync(Guid sellerId, Guid? curSellerId, UserRole role)
        {
            if (role != UserRole.Admin && curSellerId != sellerId)
                throw new UnauthorizedAccessException("Can only view own order items");
            return await _orderItemRepository.GetBySellerIdAsync(sellerId, role);
        }
    }
}