using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.GameDTO;
using Gamesbakery.Core;

namespace Gamesbakery.BusinessLogic.Services
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _gameRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthenticationService _authService;

        public GameService(
            IGameRepository gameRepository,
            ICategoryRepository categoryRepository,
            IAuthenticationService authService)
        {
            _gameRepository = gameRepository;
            _categoryRepository = categoryRepository;
            _authService = authService;
        }

        public async Task<GameDetailsDTO> AddGameAsync(Guid categoryId, string title, decimal price, DateTime releaseDate, string description, string originalPublisher)
        {
            var currentRole = _authService.GetCurrentRole();
            if (currentRole != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can add games.");

            var category = await _categoryRepository.GetByIdAsync(categoryId, currentRole);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {categoryId} not found.");

            var game = new Game(Guid.NewGuid(), categoryId, title, price, releaseDate, description, true, originalPublisher);
            var createdGame = await _gameRepository.AddAsync(game, currentRole);

            // Получаем AverageRating
            var averageRating = _gameRepository.GetGameAverageRating(createdGame.Id);

            var gameDetails = MapToDetailsDTO(createdGame);
            gameDetails.AverageRating = averageRating;
            return gameDetails;
        }

        public async Task<GameDetailsDTO> GetGameByIdAsync(Guid id)
        {
            var currentRole = _authService.GetCurrentRole();
            var game = await _gameRepository.GetByIdAsync(id, currentRole);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {id} not found.");

            // Получаем AverageRating
            var averageRating = _gameRepository.GetGameAverageRating(id);

            var gameDetails = MapToDetailsDTO(game);
            gameDetails.AverageRating = averageRating;
            return gameDetails;
        }

        public async Task<List<GameListDTO>> GetAllGamesAsync()
        {
            var currentRole = _authService.GetCurrentRole();
            var games = await _gameRepository.GetAllAsync(currentRole);
            return games.Select(MapToListDTO).ToList();
        }

        public async Task<GameDetailsDTO> SetGameForSaleAsync(Guid gameId, bool isForSale)
        {
            var currentRole = _authService.GetCurrentRole();
            if (currentRole != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can set games for sale.");

            var game = await _gameRepository.GetByIdAsync(gameId, currentRole);
            if (game == null)
                throw new KeyNotFoundException($"Game with ID {gameId} not found.");

            game.SetForSale(isForSale);
            var updatedGame = await _gameRepository.UpdateAsync(game, currentRole);

            // Получаем AverageRating
            var averageRating = _gameRepository.GetGameAverageRating(updatedGame.Id);

            var gameDetails = MapToDetailsDTO(updatedGame);
            gameDetails.AverageRating = averageRating;
            return gameDetails;
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
                OriginalPublisher = game.OriginalPublisher,
                AverageRating = 0 
            };
        }

        private GameListDTO MapToListDTO(Game game)
        {
            return new GameListDTO
            {
                Id = game.Id,
                Title = game.Title,
                Price = game.Price,
                IsForSale = game.IsForSale,
                CategoryId = game.CategoryId
            };
        }
    }
}