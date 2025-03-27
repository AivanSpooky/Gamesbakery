using Gamesbakery.Core.Entities;
using Gamesbakery.Core.Repositories;
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

        public async Task<User> AddAsync(User user)
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

        public async Task<User> GetByIdAsync(Guid id)
        {
            //if (id <= 0)
            //    throw new ArgumentException("Id must be positive.", nameof(id));

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found.");

            return user;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty.", nameof(email));

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new KeyNotFoundException($"User with email {email} not found.");

            return user;
        }

        public async Task<User> UpdateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return await _context.Users.FindAsync(user.Id);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to update user in the database.", ex);
            }
        }
    }
}