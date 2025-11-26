using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.CartDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly GamesbakeryDbContext context;

        public CartRepository(GamesbakeryDbContext context)
        {
            this.context = context;
        }

        public async Task<CarTDTO> AddAsync(CarTDTO dto, UserRole role)
        {
            var entity = new Cart(dto.CartId, dto.UserId);
            this.context.Carts.Add(entity);
            await this.context.SaveChangesAsync();
            return this.MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            var entity = await this.context.Carts.FindAsync(id);
            if (entity != null)
            {
                this.context.Carts.Remove(entity);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CarTDTO>> GetAllAsync(UserRole role)
        {
            var carts = await this.context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.OrderItem)
                .ToListAsync();
            return carts.Select(this.MapToDTO);
        }

        public async Task<CarTDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null)
        {
            var query = this.context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.OrderItem)
                .ThenInclude(oi => oi.Game)
                .AsQueryable();
            if (role == UserRole.User && userId.HasValue)
                query = query.Where(c => c.UserId == userId.Value);
            var cart = await query.FirstOrDefaultAsync(c => c.CartId == id);
            if (cart != null && role == UserRole.User && userId.HasValue && cart.UserId != userId.Value)
                return null;
            return cart != null ? this.MapToDTO(cart) : null;
        }

        public async Task<CarTDTO> UpdateAsync(CarTDTO dto, UserRole role)
        {
            var entity = await this.context.Carts.FindAsync(dto.CartId);
            if (entity == null)
                throw new KeyNotFoundException($"Cart {dto.CartId} not found");
            entity.UserId = dto.UserId;
            this.context.Carts.Update(entity);
            await this.context.SaveChangesAsync();
            return this.MapToDTO(entity);
        }

        public async Task<CarTDTO?> GetByUserIdAsync(Guid userId, UserRole role)
        {
            var query = this.context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.OrderItem)
                .AsQueryable();
            if (role == UserRole.User)
                query = query.Where(c => c.UserId == userId);
            var cart = await query.FirstOrDefaultAsync(c => c.UserId == userId);
            return cart != null ? this.MapToDTO(cart) : null;
        }

        public async Task AddItemAsync(Guid cartId, Guid orderItemId, UserRole role)
        {
            if (!await this.context.Carts.AnyAsync(c => c.CartId == cartId))
                throw new KeyNotFoundException($"Cart {cartId} not found");
            if (await this.context.CartItems.AnyAsync(ci => ci.CartID == cartId && ci.OrderItemID == orderItemId))
                return;
            if (!await this.context.OrderItems.AnyAsync(oi => oi.Id == orderItemId && oi.OrderId == null && !oi.IsGifted))
                throw new InvalidOperationException($"OrderItem {orderItemId} not available");
            var cartItem = new CartItem(Guid.NewGuid(), cartId, orderItemId);
            this.context.CartItems.Add(cartItem);
            await this.context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(Guid cartId, Guid orderItemId, UserRole role)
        {
            var cart = await this.GetByIdAsync(cartId, role);
            if (cart != null)
            {
                var item = await this.context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartID == cartId && ci.OrderItemID == orderItemId);
                if (item != null)
                {
                    this.context.CartItems.Remove(item);
                    await this.context.SaveChangesAsync();
                }
            }
        }

        public async Task ClearAsync(Guid cartId, UserRole role)
        {
            var cart = await this.GetByIdAsync(cartId, role);
            if (cart != null)
            {
                var items = await this.context.CartItems.Where(ci => ci.CartID == cartId).ToListAsync();
                this.context.CartItems.RemoveRange(items);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task RemoveCartItemsAsync(Guid cartId, List<Guid> orderItemIds, UserRole role)
        {
            var cartItemsToRemove = await this.context.CartItems
                .Where(ci => ci.CartID == cartId && orderItemIds.Contains(ci.OrderItemID))
                .ToListAsync();
            if (cartItemsToRemove.Any())
            {
                this.context.CartItems.RemoveRange(cartItemsToRemove);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task<List<CartItemDTO>> GetItemsAsync(Guid userId, UserRole role)
        {
            var cart = await this.GetByUserIdAsync(userId, role);
            if (cart == null) return new List<CartItemDTO>();
            var items = new List<CartItemDTO>();
            foreach (var cartItem in cart.Items)
            {
                var orderItem = await this.context.OrderItems
                    .Include(oi => oi.Game)
                    .Include(oi => oi.Seller)
                    .FirstOrDefaultAsync(oi => oi.Id == cartItem.OrderItemId);
                if (orderItem != null && orderItem.Game != null)
                {
                    items.Add(new CartItemDTO
                    {
                        OrderItemId = orderItem.Id,
                        GameId = orderItem.GameId,
                        GameTitle = orderItem.Game.Title,
                        GamePrice = orderItem.Game.Price,
                        Key = orderItem.Key,
                        SellerId = orderItem.SellerId,
                        SellerName = orderItem.Seller?.SellerName ?? "Unknown",
                    });
                }
            }

            return items;
        }

        private CarTDTO MapToDTO(Cart entity)
        {
            return new CarTDTO
            {
                CartId = entity.CartId,
                UserId = entity.UserId,
                Items = entity.Items.Select(this.MapToCartItemDTO).ToList(),
            };
        }

        private CartItemDTO MapToCartItemDTO(CartItem ci)
        {
            var orderItem = ci.OrderItem ?? new OrderItem();
            var game = orderItem.Game ?? new Game();
            var seller = orderItem.Seller ?? new Seller();
            return new CartItemDTO
            {
                OrderItemId = ci.OrderItemID,
                GameId = orderItem.GameId,
                GameTitle = game.Title ?? "Unknown",
                GamePrice = game.Price,
                Key = orderItem.Key,
                SellerId = orderItem.SellerId,
                SellerName = seller.SellerName ?? "Unknown",
            };
        }
    }
}
