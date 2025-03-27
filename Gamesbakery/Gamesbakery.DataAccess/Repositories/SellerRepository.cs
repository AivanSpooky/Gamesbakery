using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class SellerRepository : ISellerRepository
    {
        private readonly GamesbakeryDbContext _context;

        public SellerRepository(GamesbakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<Seller> AddAsync(Seller seller)
        {
            try
            {
                await _context.Sellers.AddAsync(seller);
                await _context.SaveChangesAsync();
                return seller;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add seller to the database.", ex);
            }
        }

        public async Task<Seller> GetByIdAsync(Guid id)
        {
            //if (id <= 0)
            //    throw new ArgumentException("Id must be positive.", nameof(id));

            var seller = await _context.Sellers.FindAsync(id);
            if (seller == null)
                throw new KeyNotFoundException($"Seller with ID {id} not found.");

            return seller;
        }

        public async Task<List<Seller>> GetAllAsync()
        {
            try
            {
                return await _context.Sellers.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve sellers from the database.", ex);
            }
        }

        public async Task<Seller> UpdateAsync(Seller seller)
        {
            if (seller == null)
                throw new ArgumentNullException(nameof(seller));

            try
            {
                _context.Sellers.Update(seller);
                await _context.SaveChangesAsync();
                return await _context.Sellers.FindAsync(seller.Id);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update seller in the database.", ex);
            }
        }
    }
}