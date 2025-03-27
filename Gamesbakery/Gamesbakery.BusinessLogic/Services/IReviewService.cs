using Gamesbakery.Core.DTOs;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IReviewService
    {
        Task<ReviewDTO> AddReviewAsync(Guid userId, Guid gameId, string text, int rating);
        Task<List<ReviewDTO>> GetReviewsByGameIdAsync(Guid gameId);
    }
}