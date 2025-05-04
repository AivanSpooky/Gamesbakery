using Gamesbakery.Core.DTOs;
using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Services
{
    public interface IUserService
    {
        Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country);
        Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country, bool proc);
        Task<UserProfileDTO> GetUserByIdAsync(Guid id);
        Task<UserProfileDTO> GetUserByEmailAsync(string email);
        Task<UserProfileDTO> UpdateBalanceAsync(Guid userId, decimal newBalance);
        Task<UserProfileDTO> BlockUserAsync(Guid userId);
        Task<UserProfileDTO> UnblockUserAsync(Guid userId);
    }
}