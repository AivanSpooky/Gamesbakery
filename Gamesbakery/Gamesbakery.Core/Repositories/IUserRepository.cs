using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core.Repositories
{
    public interface IUserRepository
    {
        Task<UserProfileDTO> AddAsync(UserProfileDTO dto, UserRole role);
        Task DeleteAsync(Guid id, UserRole role);
        Task<IEnumerable<UserProfileDTO>> GetAllAsync(UserRole role);
        Task<UserProfileDTO?> GetByIdAsync(Guid id, UserRole role);
        Task<UserProfileDTO> UpdateAsync(UserProfileDTO dto, UserRole role);
        Task<UserProfileDTO?> GetByUsernameAsync(string username, UserRole role);
        Task<UserProfileDTO?> GetProfileAsync(Guid id, UserRole role);
        decimal GetTotalSpent(Guid userId);
        Task<UserProfileDTO?> GetByEmailAsync(string email, UserRole role);
    }
}