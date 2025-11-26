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
        private readonly IGameService gameService;
        private readonly ICategoryService categoryService;
        private readonly IOrderItemService orderItemService;
        private readonly IReviewService reviewService;

        public GameController(
            IGameService gameService,
            ICategoryService categoryService,
            IOrderItemService orderItemService,
            IReviewService reviewService,
            IConfiguration configuration)
            : base(Log.ForContext<GameController>(), configuration)
        {
            this.gameService = gameService;
            this.categoryService = categoryService;
            this.orderItemService = orderItemService;
            this.reviewService = reviewService;
        }

        public async Task<IActionResult> Index(string genre = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 10)
        {
            try
            {
                var games = await this.gameService.GetFilteredGamesAsync(genre, minPrice, maxPrice, this.User.GetRole());
                var categories = await this.categoryService.GetAllCategoriesAsync();
                var totalCount = games.Count;
                var pagedGames = games.Skip((page - 1) * pageSize).Take(pageSize).Select(g => new GameListResponseDTO
                {
                    Id = g.Id,
                    Title = g.Title,
                    Price = g.Price,
                    IsForSale = g.IsForSale,
                }).ToList();
                this.ViewBag.Page = page;
                this.ViewBag.PageSize = pageSize;
                this.ViewBag.TotalCount = totalCount;
                this.ViewBag.Genre = genre;
                this.ViewBag.MinPrice = minPrice;
                this.ViewBag.MaxPrice = maxPrice;
                this.ViewBag.Categories = categories.Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description,
                }).ToList();
                return this.View(pagedGames);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error retrieving game list");
                return this.View("Error", new ErrorViewModel { ErrorMessage = "Ошибка загрузки списка игр." });
            }
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var role = this.User.GetRole();
            var curSellerId = this.User.GetSellerId();
            try
            {
                var game = await this.gameService.GetGameByIdAsync(id, role, true);
                if (game == null)
                {
                    this.LogError(new KeyNotFoundException($"Game {id} not found"), "Game not found");
                    return this.View("Error", new ErrorViewModel { ErrorMessage = "Игра не найдена." });
                }

                var categories = await this.categoryService.GetAllCategoriesAsync();
                var orderItems = await this.orderItemService.GetFilteredAsync(null, id, curSellerId, UserRole.Guest);
                var reviews = await this.reviewService.GetReviewsByGameIdAsync(id);
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
                    AverageRating = game.AverageRating,
                };
                this.ViewBag.Categories = categories.Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description,
                }).ToList();
                this.ViewBag.OrderItems = orderItems.Select(oi => new OrderItemResponseDTO
                {
                    Id = oi.Id,
                    GameId = oi.GameId,
                    GameTitle = oi.GameTitle,
                    SellerId = oi.SellerId,
                    SellerName = oi.SellerName,
                }).ToList();
                this.ViewBag.Reviews = reviews.Select(r => new ReviewResponseDTO
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    GameId = r.GameId,
                    Text = r.Text,
                    Rating = r.Rating,
                    CreationDate = r.CreationDate,
                }).ToList();
                return this.View(gameResponse);
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error retrieving game details for Id={Id}", id);
                return this.View("Error", new ErrorViewModel { ErrorMessage = $"Ошибка загрузки деталей игры: {ex.Message}" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            try
            {
                this.ViewBag.Categories = (await this.categoryService.GetAllCategoriesAsync()).Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description,
                }).ToList();
                return this.View();
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error accessing game creation page");
                return this.View("Error", new ErrorViewModel { ErrorMessage = "Ошибка загрузки формы создания игры." });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Create(GameCreateDTO game)
        {
            var role = this.User.GetRole();
            try
            {
                if (!this.ModelState.IsValid)
                {
                    this.ViewBag.Categories = (await this.categoryService.GetAllCategoriesAsync()).Select(c => new CategoryResponseDTO
                    {
                        Id = c.Id,
                        GenreName = c.GenreName,
                        Description = c.Description,
                    }).ToList();
                    return this.View(game);
                }

                var createdGame = await this.gameService.AddGameAsync(
                    game.CategoryId, game.Title, game.Price, game.ReleaseDate, game.Description, game.OriginalPublisher, role);
                return this.RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this.LogError(ex, "Error creating game with Title={Title}", game.Title);
                this.ModelState.AddModelError(string.Empty, $"Ошибка при создании игры: {ex.Message}");
                this.ViewBag.Categories = (await this.categoryService.GetAllCategoriesAsync()).Select(c => new CategoryResponseDTO
                {
                    Id = c.Id,
                    GenreName = c.GenreName,
                    Description = c.Description,
                }).ToList();
                return this.View(game);
            }
        }
    }
}
