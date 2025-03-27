using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.GameDTO;

namespace Gamesbakery.BusinessLogic.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly ICategoryRepository _categoryRepository;

        public GameService(IGameRepository gameRepository, ICategoryRepository categoryRepository)
        {
            _gameRepository = gameRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<GameDetailsDTO> AddGameAsync(Guid categoryId, string title, decimal price, DateTime releaseDate, string description, string originalPublisher)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

            // Используем Guid.NewGuid() вместо 0
            var game = new Game(Guid.NewGuid(), categoryId, title, price, releaseDate, description, true, originalPublisher);
            var createdGame = await _gameRepository.AddAsync(game);
            return MapToDetailsDTO(createdGame);
        }

        public async Task<GameDetailsDTO> GetGameByIdAsync(Guid id)
        {
            var game = await _gameRepository.GetByIdAsync(id);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {id} not found.");
            return MapToDetailsDTO(game);
        }

        public async Task<List<GameListDTO>> GetAllGamesAsync()
        {
            var games = await _gameRepository.GetAllAsync();
            return games.Select(MapToListDTO).ToList();
        }

        public async Task<GameDetailsDTO> SetGameForSaleAsync(Guid gameId, bool isForSale)
        {
            var game = await _gameRepository.GetByIdAsync(gameId);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");

            game.SetForSale(isForSale);
            var updatedGame = await _gameRepository.UpdateAsync(game);
            return MapToDetailsDTO(updatedGame);
        }

        private GameDetailsDTO MapToDetailsDTO(Game game)
        {
            return new GameDetailsDTO
            {
                Id = game.Id,
                CategoryId = game.CategoryId,
                Title = game.Title,
                Price = game.Price,
                ReleaseDate = game.ReleaseDate,
                Description = game.Description,
                IsForSale = game.IsForSale,
                OriginalPublisher = game.OriginalPublisher
            };
        }

        private GameListDTO MapToListDTO(Game game)
        {
            return new GameListDTO
            {
                Id = game.Id,
                Title = game.Title,
                Price = game.Price,
                IsForSale = game.IsForSale
            };
        }
    }
}