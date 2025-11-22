using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class OrderItemRepository : IOrderItemRepository
    {
        private readonly GamesbakeryDbContext _context;

        public OrderItemRepository(GamesbakeryDbContext context)
        {
            _context = context;
        }

        public async Task<OrderItemDTO> AddAsync(OrderItemDTO dto, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller)
                throw new UnauthorizedAccessException("Access denied.");
            var entity = new OrderItem(dto.Id, dto.OrderId, dto.GameId, dto.SellerId, dto.Key, false);
            _context.OrderItems.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task<OrderItemDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Game)
                .Include(oi => oi.Seller)
                .AsQueryable();
            if (role == UserRole.Seller && userId.HasValue)
                query = query.Where(oi => oi.SellerId == userId.Value);
            else if (role == UserRole.User && userId.HasValue)
                query = query.Where(oi => _context.Orders.Any(o => o.Id == oi.OrderId && o.UserId == userId.Value));
            else if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var entity = await query.FirstOrDefaultAsync(oi => oi.Id == id);
            return entity != null ? MapToDTO(entity) : null;
        }

        public async Task<List<OrderItemDTO>> GetByOrderIdAsync(Guid orderId, UserRole role)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Game)
                .Include(oi => oi.Seller)
                .Where(oi => oi.OrderId == orderId);
            if (role == UserRole.User)
                query = query.Where(oi => _context.Orders.Any(o => o.Id == oi.OrderId));
            var items = await query.ToListAsync();
            return items.Select(MapToDTO).ToList();
        }

        public async Task<List<OrderItemDTO>> GetBySellerIdAsync(Guid sellerId, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller)
                throw new UnauthorizedAccessException("Access denied.");
            var items = await _context.OrderItems
                .Include(oi => oi.Game)
                .Include(oi => oi.Seller)
                .Where(oi => oi.SellerId == sellerId)
                .ToListAsync();
            return items.Select(MapToDTO).ToList();
        }

        public async Task<OrderItemDTO> UpdateAsync(OrderItemDTO dto, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller)
                throw new UnauthorizedAccessException("Access denied.");
            var entity = await _context.OrderItems.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"OrderItem {dto.Id} not found.");
            entity.SetOrderId(dto.OrderId);
            entity.SetKey(dto.Key);
            _context.OrderItems.Update(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete order items.");
            var entity = await _context.OrderItems.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"OrderItem {id} not found.");
            _context.OrderItems.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetCountAsync(Guid? sellerId = null, Guid? gameId = null, UserRole role = UserRole.Admin)
        {
            var query = _context.OrderItems.AsQueryable();
            if (sellerId.HasValue)
                query = query.Where(oi => oi.SellerId == sellerId);
            if (gameId.HasValue)
                query = query.Where(oi => oi.GameId == gameId);
            return await query.CountAsync();
        }

        public async Task<List<OrderItemDTO>> GetFilteredAsync(Guid? sellerId = null, Guid? gameId = null, UserRole role = UserRole.Admin)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Game)
                .Include(oi => oi.Seller)
                .AsQueryable();
            if (sellerId.HasValue)
                query = query.Where(oi => oi.SellerId == sellerId);
            if (gameId.HasValue)
                query = query.Where(oi => oi.GameId == gameId);
            var items = await query.ToListAsync();
            return items.Select(MapToDTO).ToList();
        }

        public async Task<List<OrderItemDTO>> GetAvailableByGameIdAsync(Guid gameId, UserRole role)
        {
            var items = await _context.OrderItems
                .Include(oi => oi.Game)
                .Include(oi => oi.Seller)
                .Where(oi => oi.GameId == gameId && oi.OrderId == null && !oi.IsGifted)
                .ToListAsync();
            return items.Select(MapToDTO).ToList();
        }

        public async Task<List<OrderItemDTO>> GetByUserIdAsync(Guid userId, UserRole role)
        {
            var query = _context.OrderItems
                .Include(oi => oi.Game)
                .Include(oi => oi.Seller)
                .AsQueryable();
            if (role == UserRole.User)
                query = query.Where(oi => oi.OrderId.HasValue && _context.Orders.Any(o => o.Id == oi.OrderId && o.UserId == userId));
            var items = await query.ToListAsync();
            return items.Select(MapToDTO).ToList();
        }

        public async Task<IEnumerable<OrderItemDTO>> GetAllAsync(UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller)
                throw new UnauthorizedAccessException("Access denied.");
            var items = await _context.OrderItems
                .Include(oi => oi.Game)
                .Include(oi => oi.Seller)
                .ToListAsync();
            return items.Select(MapToDTO);
        }

        private OrderItemDTO MapToDTO(OrderItem entity)
        {
            return new OrderItemDTO
            {
                Id = entity.Id,
                OrderId = entity.OrderId,
                GameId = entity.GameId,
                SellerId = entity.SellerId,
                Key = entity.Key,
                GameTitle = entity.Game?.Title ?? "Unknown",
                SellerName = entity.Seller?.SellerName ?? "Unknown",
                GamePrice = entity.Game?.Price ?? 0
            };
        }
    }
}