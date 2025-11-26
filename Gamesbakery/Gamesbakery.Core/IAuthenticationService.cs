using Gamesbakery.Core.DTOs.UserDTO;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.Core
{
    public interface IAuthenticationService
    {
        Task<(UserRole Role, Guid? UserId, Guid? SellerId)> AuthenticateAsync(string username, string password);
        UserRole GetCurrentRole();
        Guid? GetCurrentUserId();
        Guid? GetCurrentSellerId();
        Task<UserProfileDTO> RegisterUserAsync(string username, string email, string password, string country);
    }
}
