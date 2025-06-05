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
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Reviews (ReviewID, UserID, GameID, Comment, StarRating, CreationDate) " +
                    "VALUES (@ReviewID, @UserID, @GameID, @Comment, @StarRating, @CreationDate)",
                    new SqlParameter("@ReviewID", review.Id),
                    new SqlParameter("@UserID", review.UserId),
                    new SqlParameter("@GameID", review.GameId),
                    new SqlParameter("@Comment", review.Text),
                    new SqlParameter("@StarRating", review.Rating),
                    new SqlParameter("@CreationDate", review.CreationDate));
                return review;
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
                    .FromSqlRaw("SELECT ReviewID, UserID, GameID, Comment, StarRating, CreationDate " +
                                "FROM Reviews WHERE GameID = @GameID",
                        new SqlParameter("@GameID", gameId))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve reviews for game.", ex);
            }
        }
    }
}