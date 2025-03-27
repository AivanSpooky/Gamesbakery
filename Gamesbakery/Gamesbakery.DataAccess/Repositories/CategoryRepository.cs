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

        public async Task<Category> AddAsync(Category category)
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

        public async Task<Category> GetByIdAsync(Guid id)
        {
            //if (id == 0)
            //    throw new ArgumentException("Id must be positive.", nameof(id));

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                throw new KeyNotFoundException($"Category with ID {id} not found.");

            return category;
        }

        public async Task<List<Category>> GetAllAsync()
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