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
        private readonly GamesbakeryDbContext _context;

        public CategoryRepository(GamesbakeryDbContext context)
        {
            _context = context;
        }

        public async Task<CategoryDTO> AddAsync(CategoryDTO dto, UserRole role)
        {
            var entity = new Category(dto.Id, dto.GenreName, dto.Description);
            _context.Categories.Add(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task DeleteAsync(Guid id, UserRole role)
        {
            var entity = await _context.Categories.FindAsync(id);
            if (entity != null)
            {
                _context.Categories.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<CategoryDTO>> GetAllAsync(UserRole role)
        {
            var categories = await _context.Categories.ToListAsync();
            return categories.Select(MapToDTO);
        }

        public async Task<CategoryDTO?> GetByIdAsync(Guid id, UserRole role)
        {
            var entity = await _context.Categories.FindAsync(id);
            return entity != null ? MapToDTO(entity) : null;
        }

        public async Task<CategoryDTO> UpdateAsync(CategoryDTO dto, UserRole role)
        {
            var entity = await _context.Categories.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Category {dto.Id} not found");
            entity.Update(dto.GenreName, dto.Description);
            _context.Categories.Update(entity);
            await _context.SaveChangesAsync();
            return MapToDTO(entity);
        }

        public async Task<int> GetCountAsync(UserRole role)
        {
            return await _context.Categories.CountAsync();
        }

        private CategoryDTO MapToDTO(Category entity)
        {
            return new CategoryDTO
            {
                Id = entity.Id,
                GenreName = entity.GenreName,
                Description = entity.Description
            };
        }
    }
}