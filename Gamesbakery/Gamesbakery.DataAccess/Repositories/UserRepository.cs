using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gamesbakery.Core;
using Gamesbakery.Core.DTOs.UserDTO;
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
            _context = context;
        }
        public async Task<UserProfileDTO> AddAsync(UserProfileDTO dto, UserRole role)
        {
            var entity = new User(dto.Id, dto.Username, dto.Email, dto.RegistrationDate, dto.Country, dto.Password, dto.IsBlocked, dto.Balance);
            _context.Users.Add(entity);
            await _context.SaveChangesAsync();
            return MapToProfileDTO(entity);
        }
        public async Task DeleteAsync(Guid id, UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can delete users.");
            var entity = await _context.Users.FindAsync(id);
            if (entity != null)
            {
                _context.Users.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<UserProfileDTO>> GetAllAsync(UserRole role)
        {
            if (role != UserRole.Admin)
                throw new UnauthorizedAccessException("Only admins can access all users.");
            var users = await _context.Users.ToListAsync();
            return users.Select(MapToProfileDTO);
        }
        public async Task<UserProfileDTO?> GetByIdAsync(Guid id, UserRole role)
        {
            var entity = await _context.Users.FindAsync(id);
            return entity != null ? MapToProfileDTO(entity) : null;
        }
        public async Task<UserProfileDTO> UpdateAsync(UserProfileDTO dto, UserRole role)
        {
            var entity = await _context.Users.FindAsync(dto.Id);
            if (entity == null)
                throw new KeyNotFoundException($"User {dto.Id} not found");
            entity.UpdateBalance(dto.Balance);
            entity.UpdateCountry(dto.Country);
            entity.Username = dto.Username;
            entity.Email = dto.Email;
            entity.IsBlocked = dto.IsBlocked;
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();
            return MapToProfileDTO(entity);
        }
        public async Task<UserProfileDTO?> GetByUsernameAsync(string username, UserRole role)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            return entity != null ? MapToProfileDTO(entity) : null;
        }
        public async Task<UserProfileDTO?> GetProfileAsync(Guid id, UserRole role)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (entity != null)
            {
                entity.UpdateTotalSpent(GetTotalSpent(id));
                return MapToProfileDTO(entity);
            }
            return null;
        }
        public decimal GetTotalSpent(Guid userId)
        {
            return _context.Orders
            .Where(o => o.UserId == userId && o.IsCompleted)
            .Sum(o => o.TotalAmount);
        }
        public async Task<UserProfileDTO?> GetByEmailAsync(string email, UserRole role)
        {
            var entity = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return entity != null ? MapToProfileDTO(entity) : null;
        }
        private UserProfileDTO MapToProfileDTO(User entity)
        {
            return new UserProfileDTO
            {
                Id = entity.Id,
                Username = entity.Username,
                Email = entity.Email,
                RegistrationDate = entity.RegistrationDate,
                Country = entity.Country,
                IsBlocked = entity.IsBlocked,
                Balance = entity.Balance,
                TotalSpent = GetTotalSpent(entity.Id)
            };
        }
    }
}
