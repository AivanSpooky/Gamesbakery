using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace Gamesbakery.BusinessLogic.Services
{
    public class CartService : ICartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IAuthenticationService _authService;
        private const string CartSessionKey = "Cart";
        public CartService(
        IHttpContextAccessor httpContextAccessor,
        IOrderItemRepository orderItemRepository,
        IGameRepository gameRepository,
        IAuthenticationService authService)
        {
            _authService = authService;
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _orderItemRepository = orderItemRepository ?? throw new ArgumentNullException(nameof(orderItemRepository));
            _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        }

        public async Task AddToCartAsync(Guid orderItemId)
        {
            try
            {
                var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, UserRole.User, _authService.GetCurrentUserId());
                if (orderItem == null)
                    throw new KeyNotFoundException($"OrderItem with ID {orderItemId} not found.");
                if (orderItem.OrderId != null && orderItem.OrderId != Guid.Empty)
                    throw new InvalidOperationException($"OrderItem with ID {orderItemId} is already part of an order.");
                if (orderItem.IsGifted)

                    throw new InvalidOperationException($"OrderItem with ID {orderItemId} has already been gifted.");

                var cart = GetCart();
                if (!cart.Contains(orderItemId))
                {
                    cart.Add(orderItemId);
                    SaveCart(cart);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task RemoveFromCartAsync(Guid orderItemId)
        {
            try
            {
                var cart = GetCart();
                if (cart.Remove(orderItemId))
                {
                    SaveCart(cart);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<OrderItem>> GetCartItemsAsync()
        {
            try
            {
                var cart = GetCart();
                var orderItems = new List<OrderItem>();
                var invalidItemIds = new List<Guid>();

                foreach (var orderItemId in cart)
                {
                    try
                    {
                        var orderItem = await _orderItemRepository.GetByIdAsync(orderItemId, UserRole.User, _authService.GetCurrentUserId());
                        if (orderItem != null && orderItem.OrderId == null && !orderItem.IsGifted)
                            orderItems.Add(orderItem);
                        else
                            invalidItemIds.Add(orderItemId);
                    }
                    catch (KeyNotFoundException)
                    {
                        invalidItemIds.Add(orderItemId); // Mark for removal
                    }
                }

                // Remove invalid items from cart
                if (invalidItemIds.Any())
                {
                    cart.RemoveAll(id => invalidItemIds.Contains(id));
                    SaveCart(cart);
                }

                return orderItems;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<decimal> GetCartTotalAsync()
        {
            try
            {
                var cartItems = await GetCartItemsAsync();
                decimal total = 0;
                foreach (var item in cartItems)
                {
                    try
                    {
                        var game = await _gameRepository.GetByIdAsync(item.GameId, UserRole.User);
                        if (game != null)
                        {
                            total += game.Price;
                        }
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
                return total;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public void ClearCart()
        {
            try
            {
                _httpContextAccessor.HttpContext.Session.Remove(CartSessionKey);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private List<Guid> GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var cartJson = session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson)
                ? new List<Guid>()
                : JsonConvert.DeserializeObject<List<Guid>>(cartJson);
        }

        private void SaveCart(List<Guid> cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }
    }
}
