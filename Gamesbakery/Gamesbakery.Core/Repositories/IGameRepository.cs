using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IGameRepository
    {
        Task<Game> AddAsync(Game game);
        Task<Game> GetByIdAsync(int id);
        Task<List<Game>> GetAllAsync();
        Task<Game> UpdateAsync(Game game);
    }
}