using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IReviewRepository
    {
        Task<Review> AddAsync(Review review, UserRole role);
        Task<List<Review>> GetByGameIdAsync(Guid gameId, UserRole role);
    }
}