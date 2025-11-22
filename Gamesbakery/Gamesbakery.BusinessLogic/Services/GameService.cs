using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.Core.DTOs.OrderItemDTO;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.BusinessLogic.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IOrderItemRepository _orderItemRepository;

        public GameService(IGameRepository gameRepository, ICategoryRepository categoryRepository, IOrderItemRepository orderItemRepository)
        {
            _gameRepository = gameRepository;
            _categoryRepository = categoryRepository;
            _orderItemRepository = orderItemRepository;
        }

        public async Task<GameDetailsDTO> AddGameAsync(Guid categoryId, string title, decimal price, DateTime releaseDate, string description, string originalPublisher, UserRole role, bool needAvg = true)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can add games");
            var category = await _categoryRepository.GetByIdAsync(categoryId, role);
            if (category == null)
                throw new KeyNotFoundException($"Category {categoryId} not found");
            var dto = new GameDetailsDTO
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Title = title,
                Price = price,
                ReleaseDate = releaseDate,
                Description = description,
                IsForSale = true,
                OriginalPublisher = originalPublisher
            };
            var createdGame = await _gameRepository.AddAsync(dto, role);
            if (needAvg)
                createdGame.AverageRating = _gameRepository.GetAverageRating(createdGame.Id);
            return createdGame;
        }

        public async Task<GameDetailsDTO> GetGameByIdAsync(Guid id, UserRole role)
        {
            var game = await _gameRepository.GetByIdAsync(id, role);
            if (game == null)
                throw new KeyNotFoundException($"Game {id} not found");
            game.AverageRating = _gameRepository.GetAverageRating(id);
            return game;
        }

        public async Task<GameDetailsDTO> GetGameByIdAsync(Guid id, UserRole role, bool includeOrderItems)
        {
            var game = await _gameRepository.GetByIdAsync(id, role);
            if (game == null)
                throw new KeyNotFoundException($"Game {id} not found");
            game.AverageRating = _gameRepository.GetAverageRating(id);
            if (includeOrderItems)
            {
                var orderItems = await _orderItemRepository.GetAvailableByGameIdAsync(id, role);
                game.AvailableOrderItems = orderItems.ToList();
            }
            return game;
        }

        public async Task<List<GameListDTO>> GetAllGamesAsync()
        {
            return (await _gameRepository.GetAllAsync(UserRole.Guest)).ToList();
        }

        public async Task<GameDetailsDTO> SetGameForSaleAsync(Guid gameId, bool isForSale, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can update games");
            var game = await _gameRepository.GetByIdAsync(gameId, role);
            if (game == null)
                throw new KeyNotFoundException($"Game {gameId} not found");
            game.IsForSale = isForSale;
            var updatedGame = await _gameRepository.UpdateAsync(game, role);
            updatedGame.AverageRating = _gameRepository.GetAverageRating(updatedGame.Id);
            return updatedGame;
        }

        public async Task<List<GameListDTO>> GetFilteredGamesAsync(string? genre, decimal? minPrice, decimal? maxPrice, UserRole role)
        {
            return (await _gameRepository.GetFilteredAsync(genre, minPrice, maxPrice, role)).ToList();
        }

        public async Task<int> GetFilteredGamesCountAsync(string? genre, decimal? minPrice, decimal? maxPrice, UserRole role)
        {
            return await _gameRepository.GetCountAsync(genre, minPrice, maxPrice, role);
        }

        public async Task<GameDetailsDTO> UpdateGameAsync(Guid id, Guid categoryId, string title, decimal price, DateTime releaseDate, string? description, string? originalPublisher, bool isForSale, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller)
                throw new UnauthorizedAccessException("Only admins and sellers can update games");
            var game = await _gameRepository.GetByIdAsync(id, role);
            if (game == null)
                throw new KeyNotFoundException($"Game {id} not found");
            game.CategoryId = categoryId;
            game.Title = title;
            game.Price = price;
            game.ReleaseDate = releaseDate;
            game.Description = description ?? string.Empty;
            game.OriginalPublisher = originalPublisher ?? string.Empty;
            game.IsForSale = isForSale;
            var updatedGame = await _gameRepository.UpdateAsync(game, role);
            updatedGame.AverageRating = _gameRepository.GetAverageRating(updatedGame.Id);
            return updatedGame;
        }

        public async Task<GameDetailsDTO> PartialUpdateGameAsync(Guid id, Dictionary<string, object> updates, UserRole role)
        {
            if (role != UserRole.Admin && role != UserRole.Seller)
                throw new UnauthorizedAccessException("Only admins and sellers can update games");
            var game = await _gameRepository.GetByIdAsync(id, role);
            if (game == null)
                throw new KeyNotFoundException($"Game {id} not found");
            foreach (var update in updates)
            {
                switch (update.Key.ToLower())
                {
                    case "price":
                        game.Price = Convert.ToDecimal(update.Value);
                        break;
                    case "title":
                        game.Title = update.Value?.ToString() ?? string.Empty;
                        break;
                    case "description":
                        game.Description = update.Value?.ToString() ?? string.Empty;
                        break;
                    case "categoryid":
                        game.CategoryId = Guid.Parse(update.Value.ToString());
                        break;
                    case "isforSale":
                        game.IsForSale = Convert.ToBoolean(update.Value);
                        break;
                }
            }
            var updatedGame = await _gameRepository.UpdateAsync(game, role);
            updatedGame.AverageRating = _gameRepository.GetAverageRating(updatedGame.Id);
            return updatedGame;
        }

        public async Task DeleteGameAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete games");
            await _gameRepository.DeleteAsync(id, role);
        }
    }
}