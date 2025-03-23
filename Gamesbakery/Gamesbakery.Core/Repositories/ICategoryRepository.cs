using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category> AddAsync(Category category);
        Task<Category> GetByIdAsync(int id);
        Task<List<Category>> GetAllAsync();
    }
}