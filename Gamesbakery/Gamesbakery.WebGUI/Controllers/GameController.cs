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

namespace Gamesbakery.Controllers
{
    public class GameController : BaseController
    {
        private readonly IGameService _gameService;
        private readonly ICategoryService _categoryService;
        private readonly IAuthenticationService _authService;
        private readonly IDatabaseConnectionChecker _dbChecker;

        public GameController(
            IGameService gameService,
            ICategoryService categoryService,
            IAuthenticationService authService,
            IDatabaseConnectionChecker dbChecker,
            IConfiguration configuration)
            : base(Log.ForContext<GameController>(), configuration)
        {
            _gameService = gameService;
            _categoryService = categoryService;
            _authService = authService;
            _dbChecker = dbChecker;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string genre = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User accessed game list with parameters: Page={Page}, PageSize={PageSize}, Genre={Genre}, MinPrice={MinPrice}, MaxPrice={MaxPrice} at {Timestamp}",
                        page, pageSize, genre, minPrice, maxPrice,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable at {Timestamp}",
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                        ViewBag.ErrorMessage = "База данных недоступна. Пожалуйста, попробуйте позже.";
                        return View(new List<GameListDTO>());
                    }

                    var games = await _gameService.GetAllGamesAsync();

                    if (!string.IsNullOrEmpty(genre))
                    {
                        var categories = await _categoryService.GetAllCategoriesAsync();
                        var category = categories.FirstOrDefault(c => c.GenreName.Equals(genre, StringComparison.OrdinalIgnoreCase));
                        if (category != null)
                        {
                            games = games.Where(g => g.CategoryId == category.Id).ToList();
                        }
                        else
                        {
                            games = new List<GameListDTO>();
                            LogWarning("No games found for Genre={Genre} at {Timestamp}",
                                genre,
                                TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                        }
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

                    LogInformation("Successfully retrieved {GameCount} games at {Timestamp}",
                        pagedGames.Count,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                    return View(pagedGames);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving game list at {Timestamp}",
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
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
                    LogInformation("User accessed game details with Id={Id} at {Timestamp}",
                        id,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable at {Timestamp}",
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                        ViewBag.ErrorMessage = "База данных недоступна.";
                        return View();
                    }

                    var game = await _gameService.GetGameByIdAsync(id);
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    LogInformation("Successfully retrieved game details for Id={Id} at {Timestamp}",
                        id,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                    return View(game);
                }
                catch (KeyNotFoundException ex)
                {
                    LogWarning("Game not found for Id={Id} at {Timestamp}",
                        id,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                    return NotFound();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error retrieving game details for Id={Id} at {Timestamp}",
                        id,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
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
                    LogInformation("User accessed game creation page at {Timestamp}",
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));

                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized access to game creation by Role={Role} at {Timestamp}",
                            HttpContext.Session.GetString("Role"),
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                        return Unauthorized("Только администраторы могут добавлять игры.");
                    }

                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    return View();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error accessing game creation page at {Timestamp}",
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
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
                    LogInformation("User attempted to create game with parameters: Title={Title}, CategoryId={CategoryId}, Price={Price} at {Timestamp}",
                        game.Title, game.CategoryId, game.Price,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));

                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized attempt to create game by Role={Role} at {Timestamp}",
                            HttpContext.Session.GetString("Role"),
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                        return Unauthorized("Только администраторы могут добавлять игры.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable at {Timestamp}",
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
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
                            game.OriginalPublisher);
                        LogInformation("Successfully created game Title={Title} at {Timestamp}",
                            game.Title,
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                        return RedirectToAction(nameof(Index));
                    }

                    LogWarning("Invalid model state for game creation at {Timestamp}",
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    return View(game);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error creating game Title={Title} at {Timestamp}",
                        game.Title,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                    ModelState.AddModelError("", $"Ошибка при добавлении игры: {ex.Message}");
                    ViewBag.Categories = await _categoryService.GetAllCategoriesAsync();
                    return View(game);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> SetForSale(Guid id, bool isForSale)
        {
            using (PushLogContext())
            {
                try
                {
                    LogInformation("User attempted to set game sale status with Id={Id}, IsForSale={IsForSale} at {Timestamp}",
                        id, isForSale,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));

                    if (_authService.GetCurrentRole() != UserRole.Admin)
                    {
                        LogWarning("Unauthorized attempt to set game sale status by Role={Role} at {Timestamp}",
                            HttpContext.Session.GetString("Role"),
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                        return Unauthorized("Только администраторы могут изменять статус продажи.");
                    }

                    if (!await _dbChecker.CanConnectAsync())
                    {
                        LogError("Database unavailable at {Timestamp}",
                            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                        return StatusCode(503, "База данных недоступна.");
                    }

                    await _gameService.SetGameForSaleAsync(id, isForSale);
                    LogInformation("Successfully updated sale status for game Id={Id} to IsForSale={IsForSale} at {Timestamp}",
                        id, isForSale,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    LogError(ex, "Error updating sale status for game Id={Id} at {Timestamp}",
                        id,
                        TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Russian Standard Time")));
                    ModelState.AddModelError("", $"Ошибка при обновлении статуса: {ex.Message}");
                    return RedirectToAction(nameof(Details), new { id });
                }
            }
        }
    }
}