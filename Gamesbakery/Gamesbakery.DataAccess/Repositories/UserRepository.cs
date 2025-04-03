using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.DataAccess.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly GamesbakeryDbContext _context;

        public UserRepository(GamesbakeryDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<User> AddAsync(User user, UserRole role)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
                return user;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to add user to the database.", ex);
            }
        }

        public async Task<User> AddAsync(Guid userId, string username, string email, string password, string country, DateTime registrationDate, bool isBlocked, decimal balance, UserRole role)
        {
            if (_context.Database.CurrentTransaction != null)
            {
                await _context.Database.CurrentTransaction.RollbackAsync();
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC sp_RegisterUser @UserID, @Name, @Email, @Password, @Country, @RegistrationDate, @IsBlocked, @Balance",
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@Name", username),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@Country", country),
                    new SqlParameter("@RegistrationDate", registrationDate),
                    new SqlParameter("@IsBlocked", isBlocked),
                    new SqlParameter("@Balance", balance));

                await transaction.CommitAsync();

                return await _context.Users.FindAsync(userId);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<User> GetByIdAsync(Guid userId, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    Console.WriteLine($"fff");
                    var user = await _context.Users
                        .FromSqlRaw("SELECT * FROM Users WHERE UserID = @UserID",
                            new SqlParameter("@UserID", userId))
                        .FirstOrDefaultAsync();
                    Console.WriteLine($"fff");
                    if (user == null)
                        throw new KeyNotFoundException($"User with ID {userId} not found.");
                    Console.WriteLine($"fff");
                    return user;
                }
                else
                {
                    var user = await _context.UserProfiles
                        .FirstOrDefaultAsync(u => u.Id == userId);

                    if (user == null)
                        throw new KeyNotFoundException($"User with ID {userId} not found.");

                    return user;
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve user with ID {userId}: {ex.Message}", ex);
            }

        }

        public async Task<User> UpdateAsync(User user, UserRole role)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                if (role == UserRole.Admin)
                {
                    var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Users SET Balance = @Balance, IsBlocked = @IsBlocked WHERE UserID = @UserID",
                        new SqlParameter("@Balance", user.Balance),
                        new SqlParameter("@IsBlocked", user.IsBlocked),
                        new SqlParameter("@UserID", user.Id));

                    if (rowsAffected == 0)
                        throw new KeyNotFoundException($"User with ID {user.Id} not found.");

                    return await _context.Users.FindAsync(user.Id);
                }
                else
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE UserProfile SET Balance = @Balance WHERE UserID = @UserID",
                        new SqlParameter("@Balance", user.Balance),
                        new SqlParameter("@UserID", user.Id));

                    return await _context.Users.FindAsync(user.Id);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to update user in the database: {ex.Message}", ex);
            }
        }

        public async Task<User> GetByEmailAsync(string email, UserRole role)
        {
            try
            {
                if (role == UserRole.Admin)
                {
                    return await _context.Users
                        .FirstOrDefaultAsync(u => u.Email == email);
                }
                else
                {
                    return await _context.UserProfiles
                        .FirstOrDefaultAsync(u => u.Email == email);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve user with email {email}: {ex.Message}", ex);
            }
        }
    }
}