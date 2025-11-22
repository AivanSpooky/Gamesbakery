using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IReviewService
    {
        Task<ReviewDTO> AddReviewAsync(Guid userId, Guid gameId, string text, int rating, Guid? curUserId, UserRole role);
        Task<List<ReviewDTO>> GetReviewsByGameIdAsync(Guid gameId, UserRole role = UserRole.Admin, Guid? userId = null, int? minRating = null, int? maxRating = null);
        Task<List<ReviewDTO>> GetByUserIdAsync(Guid userId, string sortByRating, UserRole role);
    }
}