using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category> AddAsync(Category category, UserRole role);
        Task<Category> GetByIdAsync(Guid id, UserRole role);
        Task<List<Category>> GetAllAsync(UserRole role);
    }
}