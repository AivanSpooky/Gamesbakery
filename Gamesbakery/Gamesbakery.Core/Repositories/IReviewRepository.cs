using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IReviewRepository
    {
        Task<Review> AddAsync(Review review);
        Task<List<Review>> GetByGameIdAsync(Guid gameId);
    }
}