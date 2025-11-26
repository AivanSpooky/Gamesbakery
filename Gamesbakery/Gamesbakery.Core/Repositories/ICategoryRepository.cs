using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs.CategoryDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface ICategoryRepository
    {
        Task<CategoryDTO> AddAsync(CategoryDTO dto, UserRole role);

        Task DeleteAsync(Guid id, UserRole role);

        Task<IEnumerable<CategoryDTO>> GetAllAsync(UserRole role);

        Task<CategoryDTO?> GetByIdAsync(Guid id, UserRole role);

        Task<CategoryDTO> UpdateAsync(CategoryDTO dto, UserRole role);

        Task<int> GetCountAsync(UserRole role);
    }
}
