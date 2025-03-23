using Gamesbakery.Core.DTOs;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IReviewService
    {
        Task<ReviewDTO> AddReviewAsync(int userId, int gameId, string text, int rating);
        Task<List<ReviewDTO>> GetReviewsByGameIdAsync(int gameId);
    }
}