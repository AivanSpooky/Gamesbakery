using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;
namespace Gamesbakery.DataAccess.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly GamesbakeryDbContext _context;
        public ReviewRepository(GamesbakeryDbContext context)
        {
            _context = context;
        }
        public async Task<ReviewDTO> AddAsync(ReviewDTO dto, UserRole role)
        {
            var entity = new Review(dto.Id, dto.UserId, dto.GameId, dto.Text, dto.Rating, dto.CreationDate);
            _context.Reviews.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }
        public async Task DeleteAsync(Guid id, UserRole role)
        {
            var entity = await _context.Reviews.FindAsync(id);
            if (entity != null)
            {
                _context.Reviews.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<ReviewDTO>> GetAllAsync(UserRole role)
        {
            var reviews = await _context.Reviews.Include(r => r.User).ToListAsync(); // Include User
            return reviews.Select(MapToDTO);
        }
        public async Task<ReviewDTO?> GetByIdAsync(Guid id, UserRole role)
        {
            var entity = await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id); // Include User
            return entity != null ? MapToDTO(entity) : null;
        }
        public async Task<ReviewDTO> UpdateAsync(ReviewDTO dto, UserRole role)
        {
            var entity = await _context.Reviews.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Review {dto.Id} not found");
            entity.Update(dto.Text, dto.Rating);
            _context.Reviews.Update(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }
        public async Task<IEnumerable<ReviewDTO>> GetByGameIdAsync(Guid gameId, UserRole role, Guid? userId = null, int? minRating = null, int? maxRating = null)
        {
            var query = _context.Reviews.Include(r => r.User).Where(r => r.GameId == gameId); // Include User
            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);
            if (minRating.HasValue)
                query = query.Where(r => r.Rating >= minRating.Value);
            if (maxRating.HasValue)
                query = query.Where(r => r.Rating <= maxRating.Value);
            var reviews = await query.ToListAsync();
            return reviews.Select(MapToDTO);
        }
        public async Task<IEnumerable<ReviewDTO>> GetByUserIdAsync(Guid userId, UserRole role)
        {
            var reviews = await _context.Reviews.Include(r => r.User).Where(r => r.UserId == userId).ToListAsync(); // Include User
            return reviews.Select(MapToDTO);
        }
        private ReviewDTO MapToDTO(Review entity)
        {
            return new ReviewDTO
            {
                Id = entity.Id,
                UserId = entity.UserId,
                GameId = entity.GameId,
                Text = entity.Text,
                Rating = entity.Rating,
                CreationDate = entity.CreationDate,
                Username = entity.User?.Username ?? "Anonymous" // Map Username, fallback if null
            };
        }
    }
}
