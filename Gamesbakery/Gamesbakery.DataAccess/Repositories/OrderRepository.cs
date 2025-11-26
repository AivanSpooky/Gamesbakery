using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.OrderDTO;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly GamesbakeryDbContext _context;

        public OrderRepository(GamesbakeryDbContext context)
        {
            _context = context;
        }

        public async Task<OrderDetailsDTO> AddAsync(OrderDetailsDTO dto, UserRole role)
        {
            if (role != UserRole.User && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var entity = new Order(dto.Id, dto.UserId, dto.OrderDate, dto.TotalPrice, "Pending", dto.IsCompleted, dto.IsOverdue);
            _context.Orders.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDetailsDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete orders.");
            var entity = await _context.Orders.FindAsync(id);
            if (entity != null)
            {
                _context.Orders.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<OrderListDTO>> GetAllAsync(UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ToListAsync();
            return orders.Select(MapToListDTO);
        }

        public async Task<OrderDetailsDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Game)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Seller)
                .AsQueryable();
            if (role == UserRole.User && userId.HasValue)
                query = query.Where(o => o.UserId == userId.Value);
            else if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var entity = await query.FirstOrDefaultAsync(o => o.Id == id);
            return entity != null ? MapToDetailsDTO(entity) : null;
        }

        public async Task<OrderDetailsDTO> UpdateAsync(OrderDetailsDTO dto, UserRole role)
        {
            if (role != UserRole.User && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var entity = await _context.Orders.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Order {dto.Id} not found");
            entity.UpdateTotalAmount(dto.TotalPrice);
            entity.Complete();
            _context.Orders.Update(entity);
            await _context.SaveChangesAsync();
            return MapToDetailsDTO(entity);
        }

        public async Task<IEnumerable<OrderListDTO>> GetByUserIdAsync(Guid userId, UserRole role)
        {
            if (role != UserRole.User && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                .ToListAsync();
            return orders.Select(MapToListDTO);
        }

        private OrderDetailsDTO MapToDetailsDTO(Order entity)
        {
            return new OrderDetailsDTO
            {
                Id = entity.Id,
                UserId = entity.UserId,
                OrderDate = entity.OrderDate,
                TotalPrice = entity.TotalAmount,
                IsCompleted = entity.IsCompleted,
                IsOverdue = entity.IsOverdue,
                OrderItems = entity.OrderItems.Select(oi => new OrderItemDTO
                {
                    Id = oi.Id,
                    OrderId = oi.OrderId,
                    GameId = oi.GameId,
                    SellerId = oi.SellerId,
                    Key = oi.Key,
                    GameTitle = oi.Game?.Title ?? "Unknown",
                    SellerName = oi.Seller?.SellerName ?? "Unknown",
                    GamePrice = oi.Game?.Price ?? 0
                }).ToList()
            };
        }

        private OrderListDTO MapToListDTO(Order entity)
        {
            return new OrderListDTO
            {
                OrderId = entity.Id,
                OrderDate = entity.OrderDate,
                TotalAmount = entity.TotalAmount,
                IsCompleted = entity.IsCompleted,
                IsOverdue = entity.IsOverdue
            };
        }
    }
}
