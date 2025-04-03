using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.Data.SqlClient;
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

        public async Task<Seller> AddAsync(Seller seller, UserRole role)
        {
            try
            {
                if (role != UserRole.Admin)
                    throw new UnauthorizedAccessException("Only administrators can add sellers.");

                await _context.Sellers.AddAsync(seller);
                await _context.SaveChangesAsync();
                return seller;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add seller to the database.", ex);
            }
        }

        public async Task<Seller> AddAsync(Guid sellerId, string sellerName, string password, DateTime registrationDate, double avgRating, UserRole role)
        {
            try
            {
                if (role != UserRole.Admin)
                    throw new UnauthorizedAccessException("Only administrators can add sellers.");

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_RegisterSeller @SellerID, @Name, @Password, @RegistrationDate, @AvgRating",
                    new SqlParameter("@SellerID", sellerId),
                    new SqlParameter("@Name", sellerName),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@RegistrationDate", registrationDate),
                    new SqlParameter("@AvgRating", avgRating));

                return await _context.Sellers.FindAsync(sellerId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add seller with ID {sellerId}: {ex.Message}", ex);
            }
        }

        public async Task<Seller> GetByIdAsync(Guid id, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    var seller = await _context.Sellers.FindAsync(id);
                    if (seller == null)
                        throw new KeyNotFoundException($"Seller with ID {id} not found.");
                    return seller;
                }
                else if (role == UserRole.Seller)
                {
                    var seller = await _context.SellerProfiles
                        .FirstOrDefaultAsync(s => s.Id == id);

                    if (seller == null)
                        throw new KeyNotFoundException($"Seller with ID {id} not found or you do not have access to this profile.");

                    return seller;
                }
                else
                {
                    throw new UnauthorizedAccessException("Only administrators and sellers can access seller profiles.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve seller with ID {id}: {ex.Message}", ex);
            }
        }

        public async Task<List<Seller>> GetAllAsync(UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    return await _context.Sellers.ToListAsync();
                }
                else if (role == UserRole.Seller)
                {
                    return await _context.SellerProfiles.ToListAsync();
                }
                else
                {
                    throw new UnauthorizedAccessException("Only administrators and sellers can access seller profiles.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve sellers from the database.", ex);
            }
        }

        public async Task<Seller> UpdateAsync(Seller seller, UserRole role)
        {
            if (seller == null)
                throw new ArgumentNullException(nameof(seller));

            try
            {
                if (role == UserRole.Admin)
                {
                    var existingSeller = await _context.Sellers.FindAsync(seller.Id);
                    if (existingSeller == null)
                        throw new KeyNotFoundException($"Seller with ID {seller.Id} not found.");

                    existingSeller.SetSellerName(seller.SellerName);
                    existingSeller.SetRegistrationDate(seller.RegistrationDate);
                    existingSeller.SetAvgRating(seller.AvgRating);
                    existingSeller.SetPassword(seller.Password);
                    await _context.SaveChangesAsync();
                    return existingSeller;
                }
                else if (role == UserRole.Seller)
                {
                    var existingSeller = await _context.SellerProfiles
                        .FirstOrDefaultAsync(s => s.Id == seller.Id);

                    if (existingSeller == null)
                        throw new KeyNotFoundException($"Seller with ID {seller.Id} not found or you do not have access to this profile.");

                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE SellerProfile SET Name = @Name, RegistrationDate = @RegistrationDate, AverageRating = @AverageRating, Password = @Password WHERE SellerID = @SellerID",
                        new SqlParameter("@Name", seller.SellerName),
                        new SqlParameter("@RegistrationDate", seller.RegistrationDate),
                        new SqlParameter("@AverageRating", seller.AvgRating),
                        new SqlParameter("@Password", seller.Password),
                        new SqlParameter("@SellerID", seller.Id));

                    return await _context.SellerProfiles.FirstOrDefaultAsync(s => s.Id == seller.Id);
                }
                else
                {
                    throw new UnauthorizedAccessException("Only administrators and sellers can update seller profiles.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update seller in the database.", ex);
            }
        }
    }
}