using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IUserRepository
    {
        Task<User> AddAsync(User user);
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<User> UpdateAsync(User user);
    }
}