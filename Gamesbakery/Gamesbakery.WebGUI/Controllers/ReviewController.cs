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
        private readonly IReviewService _reviewService;
        private readonly IGameService _gameService;

        public ReviewController(IReviewService reviewService, IGameService gameService, IConfiguration configuration)
            : base(Log.ForContext<ReviewController>(), configuration)
        {
            _reviewService = reviewService;
            _gameService = gameService;
        }

        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> UserReviews(Guid userId, int page = 1, int limit = 10, string sortByRating = null)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var role = GetCurrentRole();
                if (currentUserId != userId && role != UserRole.Admin)
                    return Forbid();
                var reviews = await _reviewService.GetByUserIdAsync(userId, sortByRating, role);
                var totalCount = reviews.Count;
                var paginatedItems = reviews.Skip((page - 1) * limit).Take(limit).Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    GameId = r.GameId,
                    Text = r.Text,
                    Rating = r.Rating,
                    CreationDate = r.CreationDate
                }).ToList();
                ViewBag.TotalCount = totalCount;
                ViewBag.Page = page;
                ViewBag.Limit = limit;
                ViewBag.UserId = userId;
                ViewBag.SortByRating = sortByRating;
                return View(paginatedItems);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading user reviews for UserId={UserId}", userId);
                ViewBag.ErrorMessage = $"Ошибка загрузки отзывов: {ex.Message}";
                return View(new List<ReviewResponseDTO>());
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(Guid gameId)
        {
            try
            {
                var reviews = await _reviewService.GetReviewsByGameIdAsync(gameId);
                var reviewsResponse = reviews.Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    GameId = r.GameId,
                    Text = r.Text,
                    Rating = r.Rating,
                    CreationDate = r.CreationDate
                }).ToList();
                ViewBag.GameId = gameId;
                return View(reviewsResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error retrieving reviews for GameId={GameId}", gameId);
                ViewBag.ErrorMessage = $"Ошибка загрузки отзывов: {ex.Message}";
                return View(new List<ReviewResponseDTO>());
            }
        }

        [Authorize(Roles = "User")]
        [HttpGet]
        public async Task<IActionResult> Create(Guid gameId)
        {
            try
            {
                var role = GetCurrentRole();
                var userId = GetCurrentUserId();
                if (userId == null)
                    return RedirectToAction("Login", "Account");
                var game = await _gameService.GetGameByIdAsync(gameId, role);
                if (game == null)
                    return NotFound();
                ViewBag.GameTitle = game.Title;
                ViewBag.GameId = gameId;
                return View(new ReviewCreateDTO { GameId = gameId });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error accessing review creation page for GameId={GameId}", gameId);
                ViewBag.ErrorMessage = $"Ошибка загрузки формы: {ex.Message}";
                return View();
            }
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(ReviewCreateDTO review)
        {
            var role = GetCurrentRole();
            var userId = GetCurrentUserId();
            try
            {
                if (userId == null)
                    return RedirectToAction("Login", "Account");
                if (!ModelState.IsValid)
                {
                    var game = await _gameService.GetGameByIdAsync(review.GameId, role);
                    ViewBag.GameTitle = game?.Title;
                    ViewBag.GameId = review.GameId;
                    return View(review);
                }
                var createdReview = await _reviewService.AddReviewAsync(userId.Value, review.GameId, review.Text, review.Rating, userId, role);
                TempData["SuccessMessage"] = "Отзыв успешно добавлен!";
                return RedirectToAction("Index", new { gameId = review.GameId });
            }
            catch (Exception ex)
            {
                LogError(ex, "Error creating review for GameId={GameId}", review.GameId);
                ModelState.AddModelError("", $"Ошибка при добавлении отзыва: {ex.Message}");
                var game = await _gameService.GetGameByIdAsync(review.GameId, role);
                ViewBag.GameTitle = game?.Title;
                ViewBag.GameId = review.GameId;
                return View(review);
            }
        }
    }
}