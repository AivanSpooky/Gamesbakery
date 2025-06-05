using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core;

namespace Gamesbakery.BusinessLogic.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IAuthenticationService _authService;

        public ReviewService(
            IReviewRepository reviewRepository,
            IUserRepository userRepository,
            IGameRepository gameRepository,
            IAuthenticationService authService)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
            _authService = authService;
        }

        public async Task<ReviewDTO> AddReviewAsync(Guid userId, Guid gameId, string comment, int starRating)
        {
            if (starRating < 1 || starRating > 5)
                throw new ArgumentException("Star rating must be between 1 and 5.", nameof(starRating));

            var review = new Review(
                Guid.NewGuid(),
                userId,
                gameId,
                comment,
                starRating,
                DateTime.UtcNow);

            await _reviewRepository.AddAsync(review, UserRole.User);

            return new ReviewDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                GameId = review.GameId,
                Text = review.Text,
                Rating = review.Rating,
                CreationDate = review.CreationDate
            };
        }

        public async Task<List<ReviewDTO>> GetReviewsByGameIdAsync(Guid gameId)
        {
            var reviews = await _reviewRepository.GetByGameIdAsync(gameId, UserRole.User);
            return reviews.Select(r => new ReviewDTO
            {
                Id = r.Id,
                UserId = r.UserId,
                GameId = r.GameId,
                Text = r.Text,
                Rating = r.Rating,
                CreationDate = r.CreationDate
            }).ToList();
        }

        private ReviewDTO MapToDTO(Review review)
        {
            return new ReviewDTO
            {
                Id = review.Id,
                UserId = review.UserId,
                GameId = review.GameId,
                Text = review.Text,
                Rating = review.Rating,
                CreationDate = review.CreationDate
            };
        }
    }
}