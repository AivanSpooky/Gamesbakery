using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.CategoryDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface ICategoryService
    {
        Task<CategoryDTO> AddCategoryAsync(string genreName, string description, UserRole currentRole);
        Task<CategoryDTO> GetCategoryByIdAsync(Guid id, UserRole currentRole);
        Task<List<CategoryDTO>> GetAllCategoriesAsync();
    }
}
