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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Review> AddAsync(Review review)
        {
            try
            {
                await _context.Reviews.AddAsync(review);
                await _context.SaveChangesAsync();
                return review;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add review to the database.", ex);
            }
        }

        public async Task<List<Review>> GetByGameIdAsync(Guid gameId)
        {
            //if (gameId <= 0)
            //    throw new ArgumentException("GameId must be positive.", nameof(gameId));

            try
            {
                return await _context.Reviews
                    .Where(r => r.GameId == gameId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve reviews for game.", ex);
            }
        }
    }
}