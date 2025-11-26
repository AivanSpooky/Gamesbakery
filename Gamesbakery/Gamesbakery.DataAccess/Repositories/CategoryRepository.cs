using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.CategoryDTO;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly GamesbakeryDbContext context;

        public CategoryRepository(GamesbakeryDbContext context)
        {
            this.context = context;
        }

        public async Task<CategoryDTO> AddAsync(CategoryDTO dto, UserRole role)
        {
            var entity = new Category(dto.Id, dto.GenreName, dto.Description);
            this.context.Categories.Add(entity);
            await this.context.SaveChangesAsync();
            return this.MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            var entity = await this.context.Categories.FindAsync(id);
            if (entity != null)
            {
                this.context.Categories.Remove(entity);
                await this.context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync(UserRole role)
        {
            var categories = await this.context.Categories.ToListAsync();
            return categories.Select(this.MapToDTO);
        }

        public async Task<CategoryDTO?> GetByIdAsync(Guid id, UserRole role)
        {
            var entity = await this.context.Categories.FindAsync(id);
            return entity != null ? this.MapToDTO(entity) : null;
        }

        public async Task<CategoryDTO> UpdateAsync(CategoryDTO dto, UserRole role)
        {
            var entity = await this.context.Categories.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Category {dto.Id} not found");
            entity.Update(dto.GenreName, dto.Description);
            this.context.Categories.Update(entity);
            await this.context.SaveChangesAsync();
            return this.MapToDTO(entity);
        }

        public async Task<int> GetCountAsync(UserRole role)
        {
            return await this.context.Categories.CountAsync();
        }

        private CategoryDTO MapToDTO(Category entity)
        {
            return new CategoryDTO
            {
                Id = entity.Id,
                GenreName = entity.GenreName,
                Description = entity.Description,
            };
        }
    }
}
