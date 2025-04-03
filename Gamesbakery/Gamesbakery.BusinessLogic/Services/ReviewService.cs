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

        public async Task<ReviewDTO> AddReviewAsync(Guid userId, Guid gameId, string text, int rating)
        {
            var currentRole = _authService.GetCurrentRole();
            var currentUserId = _authService.GetCurrentUserId();

            // Проверяем, что пользователь добавляет отзыв от своего имени, если он не администратор
            if (currentRole != UserRole.Admin && userId != currentUserId)
                throw new UnauthorizedAccessException("You can only add reviews for yourself.");

            // Проверка существования пользователя
            var user = await _userRepository.GetByIdAsync(userId, currentRole);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            if (user.IsBlocked)
                throw new InvalidOperationException("Blocked users cannot add reviews.");

            // Проверка существования игры
            var game = await _gameRepository.GetByIdAsync(gameId, currentRole);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");

            // Создание отзыва
            var review = new Review(Guid.NewGuid(), userId, gameId, text, rating, DateTime.UtcNow);
            var createdReview = await _reviewRepository.AddAsync(review, currentRole);

            return MapToDTO(createdReview);
        }

        public async Task<List<ReviewDTO>> GetReviewsByGameIdAsync(Guid gameId)
        {
            var currentRole = _authService.GetCurrentRole();

            // Проверка существования игры
            var game = await _gameRepository.GetByIdAsync(gameId, currentRole);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");

            var reviews = await _reviewRepository.GetByGameIdAsync(gameId, currentRole);
            return reviews.Select(MapToDTO).ToList();
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