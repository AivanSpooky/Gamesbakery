using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface ICategoryService
    {
        Task<Category> AddCategoryAsync(string genreName, string description);
        Task<Category> GetCategoryByIdAsync(Guid id);
        Task<List<Category>> GetAllCategoriesAsync();
    }
}