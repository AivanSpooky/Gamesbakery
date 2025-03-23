using Gamesbakery.Core.DTOs.GameDTO;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IGameService
    {
        Task<GameDetailsDTO> AddGameAsync(int categoryId, string title, decimal price, DateTime releaseDate, string description, string originalPublisher);
        Task<GameDetailsDTO> GetGameByIdAsync(int id);
        Task<List<GameListDTO>> GetAllGamesAsync();
        Task<GameDetailsDTO> SetGameForSaleAsync(int gameId, bool isForSale);
    }
}