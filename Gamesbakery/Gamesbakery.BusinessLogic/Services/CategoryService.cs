using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.CategoryDTO;
using Gamesbakery.Core.Repositories;

namespace Gamesbakery.BusinessLogic.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<CategoryDTO> AddCategoryAsync(string genreName, string description, UserRole currentRole)
        {
            if (currentRole != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can add categories");
            var dto = new CategoryDTO { Id = Guid.NewGuid(), GenreName = genreName, Description = description };
            return await _categoryRepository.AddAsync(dto, currentRole);
        }

        public async Task<CategoryDTO> GetCategoryByIdAsync(Guid id, UserRole currentRole)
        {
            var category = await _categoryRepository.GetByIdAsync(id, currentRole);
            if (category == null)
                throw new KeyNotFoundException($"Category {id} not found");
            return category;
        }

        public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
        {
            return (await _categoryRepository.GetAllAsync(UserRole.Guest)).ToList();
        }
    }
}