using Gamesbakery.Core;
using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IUserService
    {
        Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country);
        Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country, bool proc);
        Task<UserProfileDTO> GetUserByIdAsync(Guid id, Guid? curUserId, UserRole role);
        Task<UserProfileDTO> GetUserByEmailAsync(string email, Guid? curUserId, UserRole role);
        Task<UserProfileDTO> UpdateBalanceAsync(Guid userId, decimal newBalance, Guid? curUserId, UserRole role);
        Task<UserProfileDTO> BlockUserAsync(Guid userId, UserRole role);
        Task<UserProfileDTO> UnblockUserAsync(Guid userId, UserRole role);
        Task<IEnumerable<UserListDTO>> GetAllUsersExceptAsync(Guid excludedUserId, UserRole role);
        Task<UserListDTO> GetByUsernameAsync(string username, UserRole role);
    }
}