using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.Repositories;
namespace Gamesbakery.BusinessLogic.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGameRepository _gameRepository;
        public ReviewService(IReviewRepository reviewRepository, IUserRepository userRepository, IGameRepository gameRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
            _gameRepository = gameRepository;
        }
        public async Task<ReviewDTO> AddReviewAsync(Guid userId, Guid gameId, string text, int rating, Guid? curUserId, UserRole role)
        {
            if (role != UserRole.Admin && userId != curUserId)
                throw new UnauthorizedAccessException("Can only review from own account");
            var user = await _userRepository.GetByIdAsync(userId, role);
            if (user?.IsBlocked == true)
                throw new InvalidOperationException("Blocked users cannot review");
            var game = await _gameRepository.GetByIdAsync(gameId, role);
            if (game == null)
                throw new KeyNotFoundException($"Game {gameId} not found");
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be 1-5");
            var reviewDto = new ReviewDTO
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameId = gameId,
                Text = text,
                Rating = rating,
                CreationDate = DateTime.UtcNow,
                Username = user.Username // Set during creation
            };
            return await _reviewRepository.AddAsync(reviewDto, UserRole.User);
        }
        public async Task<List<ReviewDTO>> GetReviewsByGameIdAsync(Guid gameId, UserRole role = UserRole.Admin, Guid? userId = null, int? minRating = null, int? maxRating = null)
        {
            return (await _reviewRepository.GetByGameIdAsync(gameId, role, userId, minRating, maxRating)).ToList();
        }
        public async Task<List<ReviewDTO>> GetByUserIdAsync(Guid userId, string sortByRating, UserRole role)
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(userId, role);
            if (!string.IsNullOrEmpty(sortByRating))
            {
                var isAsc = sortByRating.ToLower() == "asc";
                reviews = isAsc ? reviews.OrderBy(r => r.Rating) : reviews.OrderByDescending(r => r.Rating);
            }
            return reviews.ToList();
        }
    }
}
