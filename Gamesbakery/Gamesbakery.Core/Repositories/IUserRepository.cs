using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync();
        Task<User> AddAsync(User user, UserRole role);
        Task<User> AddAsync(Guid userId, string username, string email, string password, string country, DateTime registrationDate, bool isBlocked, decimal balance, UserRole role);
        Task<User> GetByIdAsync(Guid userId, UserRole role);
        Task<User> UpdateAsync(User user, UserRole role);
        Task<User> GetByEmailAsync(string email, UserRole role);
        Task<User> GetByUsernameAsync(string username, UserRole role);
        decimal GetUserTotalSpent(Guid userId);
    }
}