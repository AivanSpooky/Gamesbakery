using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.GameDTO;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IGameService
    {
        Task<GameDetailsDTO> AddGameAsync(Guid categoryId, string title, decimal price, DateTime releaseDate, string description, string originalPublisher, UserRole role, bool needAvg = true);
        Task<GameDetailsDTO> GetGameByIdAsync(Guid id, UserRole role);
        Task<GameDetailsDTO> GetGameByIdAsync(Guid id, UserRole role, bool includeOrderItems);
        Task<List<GameListDTO>> GetAllGamesAsync();
        Task<GameDetailsDTO> SetGameForSaleAsync(Guid gameId, bool isForSale, UserRole role);
        Task<List<GameListDTO>> GetFilteredGamesAsync(string? genre, decimal? minPrice, decimal? maxPrice, UserRole role);
        Task<int> GetFilteredGamesCountAsync(string? genre, decimal? minPrice, decimal? maxPrice, UserRole role);
        Task<GameDetailsDTO> UpdateGameAsync(Guid id, Guid categoryId, string title, decimal price, DateTime releaseDate, string? description, string? originalPublisher, bool isForSale, UserRole role);
        Task<GameDetailsDTO> PartialUpdateGameAsync(Guid id, Dictionary<string, object> updates, UserRole role);
        Task DeleteGameAsync(Guid id, UserRole role);
    }
}
