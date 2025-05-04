using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Categories (CategoryID, Name, Description) VALUES (@CategoryID, @GenreName, @Description)",
                    new SqlParameter("@CategoryID", category.Id),
                    new SqlParameter("@GenreName", category.GenreName),
                    new SqlParameter("@Description", category.Description ?? (object)DBNull.Value));
                return category;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add category to the database.", ex);
            }
        }

        public async Task<Category> GetByIdAsync(Guid id, UserRole role)
        {
            try
            {
                var categories = await _context.Categories
                    .FromSqlRaw("SELECT CategoryID, Name, Description FROM Categories WHERE CategoryID = @CategoryID",
                        new SqlParameter("@CategoryID", id))
                    .ToListAsync();
                var category = categories.FirstOrDefault();
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
                return await _context.Categories
                    .FromSqlRaw("SELECT CategoryID, Name, Description FROM Categories")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve categories from the database.", ex);
            }
        }
    }
}