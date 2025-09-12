using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.BusinessLogic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Gamesbakery.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Serilog;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.Controllers
{
    public class GameController : BaseController
    {
        private readonly IGameService _gameService;
        private readonly ICategoryService _categoryService;
        private readonly IAuthenticationService _authService;
        private readonly IDatabaseConnectionChecker _dbChecker;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IReviewService _reviewService;

        public GameController(
        IGameService gameService,
        ICategoryService categoryService,
        IAuthenticationService authService,
        IDatabaseConnectionChecker dbChecker,
        IOrderItemRepository orderItemRepository,
        IReviewService reviewService,
        IConfiguration configuration)
        : base(Log.ForContext<GameController>(), configuration)
        {
            _gameService = gameService;
            _categoryService = categoryService;
            _authService = authService;
            _dbChecker = dbChecker;
            _orderItemRepository = orderItemRepository;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string genre = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed game list with parameters: Page={Page}, PageSize={PageSize}, Genre={Genre}, MinPrice={MinPrice}, MaxPrice={MaxPrice}",
                        page, pageSize, genre, minPrice, maxPrice);

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View(new List<GameListDTO>());
                    }

                    var games = await _gameService.GetAllGamesAsync();

                    if (!string.IsNullOrEmpty(genre))
                    {
                        var categories = await _categoryService.GetAllCategoriesAsync();
                        var category = categories.FirstOrDefault(c => c.GenreName.Equals(genre, StringComparison.OrdinalIgnoreCase));
                        if (category != null)
                            games = games.Where(g => g.CategoryId == category.Id).ToList();
                        else
                            games = new List<GameListDTO>();
                    }

                    if (minPrice.HasValue)
                        games = games.Where(g => g.Price >= minPrice.Value).ToList();
                    if (maxPrice.HasValue)
                        games = games.Where(g => g.Price <= maxPrice.Value).ToList();

                    var pagedGames = games.Skip((page - 1) * pageSize).Take(pageSize).ToList();
                    ViewBag.Page = page;
                    ViewBag.PageSize = pageSize;
                    ViewBag.TotalCount = games.Count;
                    ViewBag.Genre = genre;
                    ViewBag.MinPrice = minPrice;
                    ViewBag.MaxPrice = maxPrice;
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();

                    LogInformation("Successfully retrieved {GameCount} games", pagedGames.Count);
                    return View(pagedGames);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving game list");
                    throw;
                }
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed game details with Id={Id}", id);
                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View();
                    }

                    var game = await _gameService.GetGameByIdAsync(id);
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    ViewBag.OrderItems = await _orderItemRepository.GetAvailableByGameIdAsync(id);
                    ViewBag.Reviews = await _reviewService.GetReviewsByGameIdAsync(id);
                    LogInformation("Successfully retrieved game details for Id={Id}", id);
                    return View(game);
                }
                catch (KeyNotFoundException ex)
                {
                    LogWarning("Game not found for Id={Id}", id);
                    return NotFound();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving game details for Id={Id}", id);
                    throw;
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed game creation page");
                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized access to game creation by Role={Role}", HttpContext.Session.GetString("Role"));
                        return Unauthorized("Только администраторы могут добавлять игры.");
                    }

                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    return View();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing game creation page");
                    throw;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(GameDetailsDTO game)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to create game with parameters: Title={Title}, CategoryId={CategoryId}, Price={Price}",
                        game.Title, game.CategoryId, game.Price);

                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized attempt to create game by Role={Role}", HttpContext.Session.GetString("Role"));
                        return Unauthorized("Только администраторы могут добавлять игры.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable");
                        ModelState.AddModelError("", "База данных недоступна.");
                        ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                        return View(game);
                    }

                    if (ModelState.IsValid)
                    {
                        await _gameService.AddGameAsync(
                            game.CategoryId,
                            game.Title,
                            game.Price,
                            game.ReleaseDate,
                            game.Description,
                            game.OriginalPublisher,
                            false);

                        LogInformation("Successfully created game with Title={Title}", game.Title);
                        return RedirectToAction("Index");
                    }

                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    LogWarning("Invalid model state for game creation");
                    return View(game);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error creating game with Title={Title}", game.Title);
                    ModelState.AddModelError("", $"Ошибка при создании игры: {ex.Message}");
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    return View(game);
                }
            }
        }
    }
}