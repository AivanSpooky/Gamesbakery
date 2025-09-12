using Gamesbakery.Core.DTOs.GameDTO;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IGameService
    {
        Task<GameDetailsDTO> AddGameAsync(Guid categoryId, string title, decimal price, DateTime releaseDate, string description, string originalPublisher, bool needAvg);
        Task<GameDetailsDTO> GetGameByIdAsync(Guid id);
        Task<List<GameListDTO>> GetAllGamesAsync();
        Task<GameDetailsDTO> SetGameForSaleAsync(Guid gameId, bool isForSale);
    }
}