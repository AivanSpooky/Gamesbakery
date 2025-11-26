using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class GiftRepository : IGiftRepository
    {
        private readonly GamesbakeryDbContext _context;

        public GiftRepository(GamesbakeryDbContext context)
        {
            _context = context;
        }

        public async Task<GiftDTO> AddAsync(GiftDTO dto, UserRole role)
        {
            if (role != UserRole.User && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var entity = new Gift(dto.GiftId, dto.SenderId, dto.RecipientId, dto.OrderItemId, dto.GiftDate, dto.Type, dto.GameTitle, dto.Key);
            _context.Gifts.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete gifts.");
            var entity = await _context.Gifts.FindAsync(id);
            if (entity != null)
            {
                _context.Gifts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<GiftDTO?> GetByIdAsync(Guid id, UserRole role, Guid? userId = null)
        {
            var query = _context.Gifts.AsQueryable();
            if (role == UserRole.User || role == UserRole.Seller)
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("User ID required for non-admin access.");
                query = query.Where(g => g.SenderId == userId.Value || g.RecipientId == userId.Value);
            }
            else if (role != UserRole.Admin)
            {
                throw new UnauthorizedAccessException("Access denied.");
            }
            var entity = await query.FirstOrDefaultAsync(g => g.Id == id);
            return entity != null ? MapToDTO(entity) : null;
        }

        public async Task<GiftDTO> UpdateAsync(GiftDTO dto, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can update gifts.");
            var entity = await _context.Gifts.FindAsync(dto.GiftId);
            if (entity == null)
                throw new KeyNotFoundException($"Gift {dto.GiftId} not found");
            entity.SenderId = dto.SenderId;
            entity.RecipientId = dto.RecipientId;
            entity.OrderItemId = dto.OrderItemId;
            entity.GiftDate = dto.GiftDate;
            entity.Type = dto.Type;
            _context.Gifts.Update(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task<List<GiftDTO>> GetBySenderIdAsync(Guid senderId, UserRole role)
        {
            if (role != UserRole.User && role != UserRole.Seller && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var gifts = await _context.Gifts.Where(g => g.SenderId == senderId).ToListAsync();
            return gifts.Select(MapToDTO).ToList();
        }

        public async Task<List<GiftDTO>> GetByRecipientIdAsync(Guid recipientId, UserRole role)
        {
            if (role != UserRole.User && role != UserRole.Seller && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var gifts = await _context.Gifts.Where(g => g.RecipientId == recipientId).ToListAsync();
            return gifts.Select(MapToDTO).ToList();
        }

        public async Task<List<GiftDTO>> GetAllForUserAsync(Guid userId, UserRole role, GiftSource source)
        {
            if (role != UserRole.User && role != UserRole.Seller && role != UserRole.Admin)
                throw new UnauthorizedAccessException("Access denied.");
            var query = _context.Gifts.AsQueryable();
            switch (source)
            {
                case GiftSource.Sent:
                    query = query.Where(g => g.SenderId == userId && g.Type == GiftSource.Sent);
                    break;
                case GiftSource.Received:
                    query = query.Where(g => g.RecipientId == userId && g.Type == GiftSource.Received);
                    break;
                case GiftSource.All:
                    query = query.Where(g => g.SenderId == userId || g.RecipientId == userId);
                    break;
                default:
                    throw new ArgumentException("Invalid source");
            }
            var gifts = await query.ToListAsync();
            return gifts.Select(MapToDTO).ToList();
        }

        private GiftDTO MapToDTO(Gift entity)
        {
            return new GiftDTO
            {
                GiftId = entity.Id,
                SenderId = entity.SenderId,
                RecipientId = entity.RecipientId,
                OrderItemId = entity.OrderItemId,
                GiftDate = entity.GiftDate,
                Type = entity.Type,
                GameTitle = entity.GameTitle,
                Key = entity.Key,
            };
        }
    }
}
