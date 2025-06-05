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

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            try
            {
                return await _context.Users
                    .FromSqlRaw("SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM Users")
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to retrieve users from the database.", ex);
            }
        }

        public async Task<User> AddAsync(User user, UserRole role)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Users (UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance) " +
                    "VALUES (@UserID, @Name, @Email, @RegistrationDate, @Country, @Password, @IsBlocked, @Balance)",
                    new SqlParameter("@UserID", user.Id),
                    new SqlParameter("@Name", user.Username),
                    new SqlParameter("@Email", user.Email),
                    new SqlParameter("@RegistrationDate", user.RegistrationDate),
                    new SqlParameter("@Country", user.Country),
                    new SqlParameter("@Password", user.Password),
                    new SqlParameter("@IsBlocked", user.IsBlocked),
                    new SqlParameter("@Balance", user.Balance));
                return user;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to add user to the database.", ex);
            }
        }

        public async Task<User> AddAsync(Guid userId, string username, string email, string password, string country, DateTime registrationDate, bool isBlocked, decimal balance, UserRole role)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO Users (UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance) " +
                    "VALUES (@UserID, @Name, @Email, @RegistrationDate, @Country, @Password, @IsBlocked, @Balance)",
                    new SqlParameter("@UserID", userId),
                    new SqlParameter("@Name", username),
                    new SqlParameter("@Email", email),
                    new SqlParameter("@RegistrationDate", registrationDate),
                    new SqlParameter("@Country", country),
                    new SqlParameter("@Password", password),
                    new SqlParameter("@IsBlocked", isBlocked),
                    new SqlParameter("@Balance", balance));

                await transaction.CommitAsync();
                return await GetByIdAsync(userId, role);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException($"Failed to add user with ID {userId}: {ex.Message}", ex);
            }
        }

        public async Task<User> GetByIdAsync(Guid userId, UserRole role)
        {
            try
            {
                string query = role == UserRole.Admin
                    ? "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM Users WHERE UserID = @UserID"
                    : "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM UserProfile WHERE UserID = @UserID";

                var users = await _context.Users
                    .FromSqlRaw(query, new SqlParameter("@UserID", userId))
                    .ToListAsync();
                var user = users.FirstOrDefault();
                if (user == null)
                    throw new KeyNotFoundException($"User with ID {userId} not found.");
                return user;
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
                string query = role == UserRole.Admin
                    ? "UPDATE Users SET Name = @Name, Email = @Email, RegistrationDate = @RegistrationDate, Country = @Country, " +
                      "Password = @Password, IsBlocked = @IsBlocked, Balance = @Balance WHERE UserID = @UserID"
                    : "UPDATE Users SET Balance = @Balance WHERE UserID = @UserID";

                SqlParameter[] parameters = role == UserRole.Admin
                    ? new[]
                    {
                    new SqlParameter("@Name", user.Username),
                    new SqlParameter("@Email", user.Email),
                    new SqlParameter("@RegistrationDate", user.RegistrationDate),
                    new SqlParameter("@Country", user.Country),
                    new SqlParameter("@Password", user.Password),
                    new SqlParameter("@IsBlocked", user.IsBlocked),
                    new SqlParameter("@Balance", user.Balance),
                    new SqlParameter("@UserID", user.Id)
                    }
                    : new[]
                    {
                    new SqlParameter("@Balance", user.Balance),
                    new SqlParameter("@UserID", user.Id)
                    };

                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(query, parameters);
                if (rowsAffected == 0)
                    throw new KeyNotFoundException($"User with ID {user.Id} not found.");

                return await GetByIdAsync(user.Id, role);
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
                string query = role == UserRole.Admin
                    ? "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM Users WHERE Email = @Email"
                    : "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM UserProfile WHERE Email = @Email";

                var users = await _context.Users
                    .FromSqlRaw(query, new SqlParameter("@Email", email))
                    .ToListAsync();
                return users.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve user with email {email}: {ex.Message}", ex);
            }
        }

        public async Task<User> GetByUsernameAsync(string username, UserRole role)
        {
            try
            {
                string query = role == UserRole.Admin
                    ? "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM Users WHERE Name = @Name"
                    : "SELECT UserID, Name, Email, RegistrationDate, Country, Password, IsBlocked, Balance FROM UserProfile WHERE Name = @Name";

                var users = await _context.Users
                    .FromSqlRaw(query, new SqlParameter("@Name", username))
                    .ToListAsync();
                return users.FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve user with username {username}: {ex.Message}", ex);
            }
        }
        public decimal GetUserTotalSpent(Guid userId)
        {
            return _context.GetUserTotalSpent(userId);
        }
    }
}