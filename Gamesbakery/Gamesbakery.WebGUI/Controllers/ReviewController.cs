using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Gamesbakery.Infrastructure;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Gamesbakery.Controllers
{
    public class ReviewController : BaseController
    {
        private readonly IReviewService _reviewService;
        private readonly IGameService _gameService;
        private readonly IAuthenticationService _authService;
        private readonly IDatabaseConnectionChecker _dbChecker;

        public ReviewController(
            IReviewService reviewService,
            IGameService gameService,
            IAuthenticationService authService,
            IDatabaseConnectionChecker dbChecker,
            IConfiguration configuration)
            : base(Log.ForContext<ReviewController>(), configuration)
        {
            _reviewService = reviewService;
            _gameService = gameService;
            _authService = authService;
            _dbChecker = dbChecker;
        }

        public async Task<IActionResult> Index(Guid gameId)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed reviews for GameId={GameId}", gameId);
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View(new List<ReviewDTO>());
                    }

                    var reviews = await _reviewService.GetReviewsByGameIdAsync(gameId);
                    ViewBag.GameId = gameId;
                    LogInformation("Successfully retrieved reviews for GameId={GameId}", gameId);
                    return View(reviews);
                }
                catch (KeyNotFoundException ex)
                {
                    LogWarning("No reviews found for GameId={GameId}", gameId);
                    return NotFound();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving reviews for GameId={GameId}", gameId);
                    throw;
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid gameId)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed review creation page for GameId={GameId}", gameId);
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized access to review creation");
                        return Unauthorized("Требуется авторизация.");
                    }

                    var game = await _gameService.GetGameByIdAsync(gameId);
                    ViewBag.GameTitle = game.Title;
                    return View(new ReviewDTO { GameId = gameId, Text = "" });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing review creation page for GameId={GameId}", gameId);
                    throw;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReviewDTO review)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to create review with parameters: GameId={GameId}, Rating={Rating}, Text={Text}",
                        review.GameId, review.Rating, review.Text);
                    if (_authService.GetCurrentUserId() == null)
                    {
                        LogWarning("Unauthorized attempt to create review");
                        return Unauthorized("Требуется авторизация.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ModelState.AddModelError("", "База данных недоступна.");
                        var game = await _gameService.GetGameByIdAsync(review.GameId);
                        ViewBag.GameTitle = game.Title;
                        return View(review);
                    }

                    if (ModelState.IsValid)
                    {
                        var userId = _authService.GetCurrentUserId().Value;
                        await _reviewService.AddReviewAsync(userId, review.GameId, review.Text, review.Rating);
                        LogInformation("Successfully created review for GameId={GameId} by UserId={UserId}", review.GameId, userId);
                        return RedirectToAction(nameof(Index), new { gameId = review.GameId });
                    }

                    LogWarning("Invalid model state for review creation");
                    var gameForError = await _gameService.GetGameByIdAsync(review.GameId);
                    ViewBag.GameTitle = gameForError.Title;
                    return View(review);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error creating review for GameId={GameId}", review.GameId);
                    ModelState.AddModelError("", $"Ошибка при добавлении отзыва: {ex.Message}");
                    var gameForError = await _gameService.GetGameByIdAsync(review.GameId);
                    ViewBag.GameTitle = gameForError.Title;
                    return View(review);
                }
            }
        }
    }
}