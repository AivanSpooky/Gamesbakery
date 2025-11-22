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
        private readonly GamesbakeryDbContext _context;

        public CartRepository(GamesbakeryDbContext context)
        {
            _context = context;
        }

        public async Task<CarTDTO> AddAsync(CarTDTO dto, UserRole role)
        {
            var entity = new Cart(dto.CartId, dto.UserId);
            _context.Carts.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            var entity = await _context.Carts.FindAsync(id);
            if (entity != null)
            {
                _context.Carts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CarTDTO>> GetAllAsync(UserRole role)
        {
            var carts = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.OrderItem)
                .ToListAsync();
            return carts.Select(MapToDTO);
        }

        public async Task<CarTDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null)
        {
            var query = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.OrderItem)
                .ThenInclude(oi => oi.Game)
                .AsQueryable();
            if (role == UserRole.User && userId.HasValue)
                query = query.Where(c => c.UserId == userId.Value);
            var cart = await query.FirstOrDefaultAsync(c => c.CartId == id);
            if (cart != null && role == UserRole.User && userId.HasValue && cart.UserId != userId.Value)
                return null;
            return cart != null ? MapToDTO(cart) : null;
        }

        public async Task<CarTDTO> UpdateAsync(CarTDTO dto, UserRole role)
        {
            var entity = await _context.Carts.FindAsync(dto.CartId);
            if (entity == null)
                throw new KeyNotFoundException($"Cart {dto.CartId} not found");
            entity.UserId = dto.UserId;
            _context.Carts.Update(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task<CarTDTO?> GetByUserIdAsync(Guid userId, UserRole role)
        {
            var query = _context.Carts
                .Include(c => c.Items)
                .ThenInclude(ci => ci.OrderItem)
                .AsQueryable();
            if (role == UserRole.User)
                query = query.Where(c => c.UserId == userId);
            var cart = await query.FirstOrDefaultAsync(c => c.UserId == userId);
            return cart != null ? MapToDTO(cart) : null;
        }

        public async Task AddItemAsync(Guid cartId, Guid orderItemId, UserRole role)
        {
            if (!await _context.Carts.AnyAsync(c => c.CartId == cartId))
                throw new KeyNotFoundException($"Cart {cartId} not found");
            if (await _context.CartItems.AnyAsync(ci => ci.CartID == cartId && ci.OrderItemID == orderItemId))
                return;
            if (!await _context.OrderItems.AnyAsync(oi => oi.Id == orderItemId && oi.OrderId == null && !oi.IsGifted))
                throw new InvalidOperationException($"OrderItem {orderItemId} not available");
            var cartItem = new CartItem(Guid.NewGuid(), cartId, orderItemId);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(Guid cartId, Guid orderItemId, UserRole role)
        {
            var cart = await GetByIdAsync(cartId, role);
            if (cart != null)
            {
                var item = await _context.CartItems
                    .FirstOrDefaultAsync(ci => ci.CartID == cartId && ci.OrderItemID == orderItemId);
                if (item != null)
                {
                    _context.CartItems.Remove(item);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task ClearAsync(Guid cartId, UserRole role)
        {
            var cart = await GetByIdAsync(cartId, role);
            if (cart != null)
            {
                var items = await _context.CartItems.Where(ci => ci.CartID == cartId).ToListAsync();
                _context.CartItems.RemoveRange(items);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveCartItemsAsync(Guid cartId, List<Guid> orderItemIds, UserRole role)
        {
            var cartItemsToRemove = await _context.CartItems
                .Where(ci => ci.CartID == cartId && orderItemIds.Contains(ci.OrderItemID))
                .ToListAsync();
            if (cartItemsToRemove.Any())
            {
                _context.CartItems.RemoveRange(cartItemsToRemove);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CartItemDTO>> GetItemsAsync(Guid userId, UserRole role)
        {
            var cart = await GetByUserIdAsync(userId, role);
            if (cart == null) return new List<CartItemDTO>();
            var items = new List<CartItemDTO>();
            foreach (var cartItem in cart.Items)
            {
                var orderItem = await _context.OrderItems
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
                        SellerName = orderItem.Seller?.SellerName ?? "Unknown"
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
                Items = entity.Items.Select(ci => new CartItemDTO
                {
                    OrderItemId = ci.OrderItemID,
                    GameId = ci.OrderItem?.GameId ?? Guid.Empty,
                    GameTitle = ci.OrderItem?.Game?.Title ?? "Unknown",
                    GamePrice = ci.OrderItem?.Game?.Price ?? 0,
                    Key = ci.OrderItem?.Key,
                    SellerId = ci.OrderItem?.SellerId ?? Guid.Empty,
                    SellerName = ci.OrderItem?.Seller?.SellerName ?? "Unknown"
                }).ToList()
            };
        }
    }
}