namespace Gamesbakery.BusinessLogic.Services
{
    public interface IUserService
    {
        Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country);
        Task<UserProfileDTO> GetUserByIdAsync(Guid id);
        Task<UserProfileDTO> GetUserByEmailAsync(string email);
        Task<UserProfileDTO> UpdateBalanceAsync(Guid userId, decimal newBalance);
        Task BlockUserAsync(Guid userId);
        Task UnblockUserAsync(Guid userId);
    }
}