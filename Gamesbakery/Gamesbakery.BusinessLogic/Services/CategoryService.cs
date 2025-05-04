using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gamesbakery.BusinessLogic.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthenticationService _authService;

        public CategoryService(ICategoryRepository categoryRepository, IAuthenticationService authService)
        {
            _categoryRepository = categoryRepository;
            _authService = authService;
        }

        public async Task<Category> AddCategoryAsync(string genreName, string description)
        {
            var currentRole = _authService.GetCurrentRole();
            if (currentRole != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can add categories.");

            var category = new Category(Guid.NewGuid(), genreName, description);
            return await _categoryRepository.AddAsync(category, currentRole);
        }

        public async Task<Category> GetCategoryByIdAsync(Guid id)
        {
            var currentRole = _authService.GetCurrentRole();
            var category = await _categoryRepository.GetByIdAsync(id, currentRole);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found.");
            return category;
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var currentRole = _authService.GetCurrentRole();
            return await _categoryRepository.GetAllAsync(currentRole);
        }
    }
}