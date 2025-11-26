using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.CartDTO;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.BusinessLogic.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;

        private readonly IOrderItemRepository _orderItemRepository;

        public CartService(ICartRepository cartRepository, IOrderItemRepository orderItemRepository)
        {
            _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
        }

        public async Task AddToCartAsync(Guid orderItemId, Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");
            var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, UserRole.Admin);
            if (orderItem == null)
                throw new KeyNotFoundException($"OrderItem {orderItemId} not found");
            if (orderItem.OrderId != null && orderItem.OrderId != Guid.Empty)
                throw new InvalidOperationException("OrderItem already in order");
            var cart = await _cartRepository.GetByUserIdAsync(userId.Value, UserRole.User);
            if (cart == null)
            {
                cart = new CarTDTO { CartId = Guid.NewGuid(), UserId = userId.Value };
                await _cartRepository.AddAsync(cart, UserRole.User);
            }
            int maxRetries = 3;
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await _cartRepository.AddItemAsync(cart.CartId, orderItemId, UserRole.User);
                    return;
                }
                catch (Exception)
                {
                    if (i == maxRetries - 1) throw;
                    await Task.Delay(100 * (i + 1));
                }
            }
        }

        public async Task RemoveFromCartAsync(Guid orderItemId, Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");
            var cart = await _cartRepository.GetByUserIdAsync(userId.Value, UserRole.User);
            if (cart != null)
                await _cartRepository.RemoveItemAsync(cart.CartId, orderItemId, UserRole.User);
        }

        public async Task<List<CartItemDTO>> GetCartItemsAsync(Guid? userId)
        {
            if (userId == null || userId == Guid.Empty)
                return new List<CartItemDTO>();
            return await _cartRepository.GetItemsAsync(userId.Value, UserRole.User);
        }

        public async Task<decimal> GetCartTotalAsync(Guid? userId)
        {
            var cartItems = await GetCartItemsAsync(userId);
            return cartItems.Sum(item => item.GamePrice);
        }

        public async Task ClearCartAsync(Guid? userId)
        {
            if (userId != null && userId != Guid.Empty)
            {
                var cart = await _cartRepository.GetByUserIdAsync(userId.Value, UserRole.User);
                if (cart != null)
                    await _cartRepository.ClearAsync(cart.CartId, UserRole.User);
            }
        }

        public void ClearCart(Guid? userId) => ClearCartAsync(userId).GetAwaiter().GetResult();
    }
}
