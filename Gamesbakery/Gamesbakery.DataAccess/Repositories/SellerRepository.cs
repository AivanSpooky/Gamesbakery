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
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can add sellers.");

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Sellers (SellerID, Name, RegistrationDate, AverageRating, Password) " +
                    "VALUES (@SellerID, @Name, @RegistrationDate, @AverageRating, @Password)",
                    new SqlParameter("@SellerID", seller.Id),
                    new SqlParameter("@Name", seller.SellerName),
                    new SqlParameter("@RegistrationDate", seller.RegistrationDate),
                    new SqlParameter("@AverageRating", seller.AvgRating),
                    new SqlParameter("@Password", seller.Password));
                return seller;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add seller to the database.", ex);
            }
        }

        public async Task<Seller> AddAsync(Guid sellerId, string sellerName, string password, DateTime registrationDate, double avgRating, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only administrators can add sellers.");

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Sellers (SellerID, Name, RegistrationDate, AverageRating, Password) " +
                    "VALUES (@SellerID, @Name, @RegistrationDate, @AverageRating, @Password)",
                    new SqlParameter("@SellerID", sellerId),
                    new SqlParameter("@Name", sellerName),
                    new SqlParameter("@RegistrationDate", registrationDate),
                    new SqlParameter("@AverageRating", avgRating),
                    new SqlParameter("@Password", password));

                return await GetByIdAsync(sellerId, role);
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
                string query = role == UserRole.Admin
                    ? "SELECT SellerID, Name, RegistrationDate, AverageRating, Password FROM Sellers WHERE SellerID = @SellerID"
                    : "SELECT SellerID, Name, RegistrationDate, AverageRating, Password FROM SellerProfile WHERE SellerID = @SellerID";

                var sellers = await _context.Sellers
                    .FromSqlRaw(query, new SqlParameter("@SellerID", id))
                    .ToListAsync();
                var seller = sellers.FirstOrDefault();

                if (seller == null)
                    throw new KeyNotFoundException($"Seller with ID {id} not found or you do not have access to this profile.");
                return seller;
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
                string query = role == UserRole.Admin
                    ? "SELECT SellerID, Name, RegistrationDate, AverageRating, Password FROM Sellers"
                    : "SELECT SellerID, Name, RegistrationDate, AverageRating, Password FROM SellerProfile";

                return await _context.Sellers
                    .FromSqlRaw(query)
                    .ToListAsync();
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
                string query = role == UserRole.Admin
                    ? "UPDATE Sellers SET Name = @Name, RegistrationDate = @RegistrationDate, AverageRating = @AverageRating, Password = @Password WHERE SellerID = @SellerID"
                    : "UPDATE Sellers SET Name = @Name, RegistrationDate = @RegistrationDate, AverageRating = @AverageRating, Password = @Password WHERE SellerID = @SellerID";

                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    query,
                    new SqlParameter("@Name", seller.SellerName),
                    new SqlParameter("@RegistrationDate", seller.RegistrationDate),
                    new SqlParameter("@AverageRating", seller.AvgRating),
                    new SqlParameter("@Password", seller.Password),
                    new SqlParameter("@SellerID", seller.Id));

                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"Seller with ID {seller.Id} not found or you do not have access to this profile.");

                return await GetByIdAsync(seller.Id, role);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to update seller in the database.", ex);
            }
        }
    }
}