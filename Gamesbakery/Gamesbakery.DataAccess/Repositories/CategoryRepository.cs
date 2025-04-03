using Gamesbakery.Core;
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
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Category> AddAsync(Category category, UserRole role)
        {
            try
            {
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return category;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add category to the database.", ex);
            }
        }

        public async Task<Category> GetByIdAsync(Guid id, UserRole role)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);
                if (category == null)
                    throw new KeyNotFoundException($"Category with ID {id} not found.");

                return category;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve category with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<List<Category>> GetAllAsync(UserRole role)
        {
            try
            {
                return await _context.Categories.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve categories from the database.", ex);
            }
        }
    }
}