using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    [AllowAnonymous]
    public class ReviewController : BaseController
    {
        private readonly IReviewService reviewService;
        private readonly IGameService gameService;

        public ReviewController(IReviewService reviewService, IGameService gameService, IConfiguration configuration)
            : base(Log.ForContext<ReviewController>(), configuration)
        {
            this.reviewService = reviewService;
            this.gameService = gameService;
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UserReviews(Guid userId, int page = 1, int limit = 10, string sortByRating = null)
        {
            try
            {
                var currentUserId = this.GetCurrentUserId();
                var role = this.GetCurrentRole();
                if (currentUserId != userId && role != UserRole.Admin)
                    return this.Forbid();
                var reviews = await this.reviewService.GetByUserIdAsync(userId, sortByRating, role);
                var totalCount = reviews.Count;
                var paginatedItems = reviews.Skip((page - 1) * limit).Take(limit).Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    GameId = r.GameId,
                    Text = r.Text,
                    Rating = r.Rating,
                    CreationDate = r.CreationDate,
                }).ToList();
                this.ViewBag.TotalCount = totalCount;
                this.ViewBag.Page = page;
                this.ViewBag.Limit = limit;
                this.ViewBag.UserId = userId;
                this.ViewBag.SortByRating = sortByRating;
                return this.View(paginatedItems);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error loading user reviews for UserId={UserId}", userId);
                this.ViewBag.ErrorMessage = $"Ошибка загрузки отзывов: {ex.Message}";
                return this.View(new List<ReviewResponseDTO>());
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(Guid gameId)
        {
            try
            {
                var reviews = await this.reviewService.GetReviewsByGameIdAsync(gameId);
                var reviewsResponse = reviews.Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    GameId = r.GameId,
                    Text = r.Text,
                    Rating = r.Rating,
                    CreationDate = r.CreationDate,
                }).ToList();
                this.ViewBag.GameId = gameId;
                return this.View(reviewsResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error retrieving reviews for GameId={GameId}", gameId);
                this.ViewBag.ErrorMessage = $"Ошибка загрузки отзывов: {ex.Message}";
                return this.View(new List<ReviewResponseDTO>());
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> Create(Guid gameId)
        {
            try
            {
                var role = this.GetCurrentRole();
                var userId = this.GetCurrentUserId();
                if (userId == null)
                    return this.RedirectToAction("Login", "Account");
                var game = await this.gameService.GetGameByIdAsync(gameId, role);
                if (game == null)
                    return this.NotFound();
                this.ViewBag.GameTitle = game.Title;
                this.ViewBag.GameId = gameId;
                return this.View(new ReviewCreateDTO { GameId = gameId });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error accessing review creation page for GameId={GameId}", gameId);
                this.ViewBag.ErrorMessage = $"Ошибка загрузки формы: {ex.Message}";
                return this.View();
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(ReviewCreateDTO review)
        {
            var role = this.GetCurrentRole();
            var userId = this.GetCurrentUserId();
            try
            {
                if (userId == null)
                    return this.RedirectToAction("Login", "Account");
                if (!this.ModelState.IsValid)
                {
                    var game = await this.gameService.GetGameByIdAsync(review.GameId, role);
                    this.ViewBag.GameTitle = game?.Title;
                    this.ViewBag.GameId = review.GameId;
                    return this.View(review);
                }

                var createdReview = await this.reviewService.AddReviewAsync(userId.Value, review.GameId, review.Text, review.Rating, userId, role);
                this.TempData["SuccessMessage"] = "Отзыв успешно добавлен!";
                return this.RedirectToAction("Index", new { gameId = review.GameId });
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error creating review for GameId={GameId}", review.GameId);
                this.ModelState.AddModelError(string.Empty, $"Ошибка при добавлении отзыва: {ex.Message}");
                var game = await this.gameService.GetGameByIdAsync(review.GameId, role);
                this.ViewBag.GameTitle = game?.Title;
                this.ViewBag.GameId = review.GameId;
                return this.View(review);
            }
        }
    }
}
