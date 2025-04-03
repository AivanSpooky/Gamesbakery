using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.Data.SqlClient;
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

        public async Task<Review> AddAsync(Review review, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    await _context.Reviews.AddAsync(review);
                    await _context.SaveChangesAsync();
                    return review;
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO UserReviews (ReviewID, UserID, GameID, Comment, StarRating, CreationDate) " +
                        "VALUES (@ReviewID, @UserID, @GameID, @Comment, @StarRating, @CreationDate)",
                        new SqlParameter("@ReviewID", review.Id),
                        new SqlParameter("@UserID", review.UserId),
                        new SqlParameter("@GameID", review.GameId),
                        new SqlParameter("@Comment", review.Text),
                        new SqlParameter("@StarRating", review.Rating),
                        new SqlParameter("@CreationDate", review.CreationDate));

                    return review;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add review to the database.", ex);
            }
        }

        public async Task<List<Review>> GetByGameIdAsync(Guid gameId, UserRole role)
        {
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