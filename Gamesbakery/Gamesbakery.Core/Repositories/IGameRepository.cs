using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IGameRepository
    {
        Task<Game> AddAsync(Game game, UserRole role);
        Task<Game> GetByIdAsync(Guid id, UserRole role);
        Task<List<Game>> GetAllAsync(UserRole role);
        Task<Game> UpdateAsync(Game game, UserRole role);
    }
}