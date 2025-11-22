using System;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.BusinessLogic.Services;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.WebGUI.Extensions;
using Gamesbakery.WebGUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Gamesbakery.Controllers
{
    [AllowAnonymous]
    public class GameController : BaseController
    {
        private readonly IGameService _gameService;
        private readonly ICategoryService _categoryService;
        private readonly IOrderItemService _orderItemService;
        private readonly IReviewService _reviewService;

        public GameController(
            IGameService gameService,
            ICategoryService categoryService,
            IOrderItemService orderItemService,
            IReviewService reviewService,
            IConfiguration configuration)
            : base(Log.ForContext<GameController>(), configuration)
        {
            _gameService = gameService;
            _categoryService = categoryService;
            _orderItemService = orderItemService;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index(string genre = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var games = await _gameService.GetFilteredGamesAsync(genre, minPrice, maxPrice, User.GetRole());
                var categories = await _categoryService.GetAllCategoriesAsync();
                var totalCount = games.Count;
                var pagedGames = games.Skip((page - 1) * pageSize).Take(pageSize).Select(g => new GameListResponseDTO
                {
                    Id = g.Id,
                    Title = g.Title,
                    Price = g.Price,
                    IsForSale = g.IsForSale
                }).ToList();
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalCount = totalCount;
                ViewBag.Genre = genre;
                ViewBag.MinPrice = minPrice;
                ViewBag.MaxPrice = maxPrice;
                ViewBag.Categories = categories.Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description
                }).ToList();
                return View(pagedGames);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error retrieving game list");
                return View("Error", new ErrorViewModel { ErrorMessage = "Ошибка загрузки списка игр." });
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var role = User.GetRole();
            var curSellerId = User.GetSellerId();
            try
            {
                var game = await _gameService.GetGameByIdAsync(id, role, true);
                if (game == null)
                {
                    LogError(new KeyNotFoundException($"Game {id} not found"), "Game not found");
                    return View("Error", new ErrorViewModel { ErrorMessage = "Игра не найдена." });
                }
                var categories = await _categoryService.GetAllCategoriesAsync();
                var orderItems = await _orderItemService.GetFilteredAsync(null, id, curSellerId, UserRole.Guest);
                var reviews = await _reviewService.GetReviewsByGameIdAsync(id);
                var gameResponse = new GameDetailsResponseDTO
                {
                    Id = game.Id,
                    CategoryId = game.CategoryId,
                    Title = game.Title,
                    Price = game.Price,
                    ReleaseDate = game.ReleaseDate,
                    Description = game.Description,
                    IsForSale = game.IsForSale,
                    OriginalPublisher = game.OriginalPublisher,
                    AverageRating = game.AverageRating
                };
                ViewBag.Categories = categories.Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description
                }).ToList();
                ViewBag.OrderItems = orderItems.Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    GameId = oi.GameId,
                    GameTitle = oi.GameTitle,
                    SellerId = oi.SellerId,
                    SellerName = oi.SellerName
                }).ToList();
                ViewBag.Reviews = reviews.Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    GameId = r.GameId,
                    Text = r.Text,
                    Rating = r.Rating,
                    CreationDate = r.CreationDate
                }).ToList();
                return View(gameResponse);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error retrieving game details for Id={Id}", id);
                return View("Error", new ErrorViewModel { ErrorMessage = $"Ошибка загрузки деталей игры: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                ViewBag.Categories = (await _categoryService.GetAllCategoriesAsync()).Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description
                }).ToList();
                return View();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error accessing game creation page");
                return View("Error", new ErrorViewModel { ErrorMessage = "Ошибка загрузки формы создания игры." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(GameCreateDTO game)
        {
            var role = User.GetRole();
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = (await _categoryService.GetAllCategoriesAsync()).Select(c => new CategoryResponseDTO
                    {
                        Id = c.Id,
                        GenreName = c.GenreName,
                        Description = c.Description
                    }).ToList();
                    return View(game);
                }
                var createdGame = await _gameService.AddGameAsync(
                    game.CategoryId, game.Title, game.Price, game.ReleaseDate,
                    game.Description, game.OriginalPublisher, role);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error creating game with Title={Title}", game.Title);
                ModelState.AddModelError("", $"Ошибка при создании игры: {ex.Message}");
                ViewBag.Categories = (await _categoryService.GetAllCategoriesAsync()).Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description
                }).ToList();
                return View(game);
            }
        }
    }
}