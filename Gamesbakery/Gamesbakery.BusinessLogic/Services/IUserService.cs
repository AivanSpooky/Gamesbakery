namespace Gamesbakery.BusinessLogic.Services
{
    public interface IUserService
    {
        Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country);
        Task<UserProfileDTO> GetUserByIdAsync(int id);
        Task<UserProfileDTO> GetUserByEmailAsync(string email);
        Task<UserProfileDTO> UpdateBalanceAsync(int userId, decimal newBalance);
        Task BlockUserAsync(int userId);
        Task UnblockUserAsync(int userId);
    }
}