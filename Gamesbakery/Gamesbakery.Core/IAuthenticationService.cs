namespace Gamesbakery.Core
{
    public interface IAuthenticationService
    {
        Task<(UserRole Role, Guid? UserId, Guid? SellerId)> AuthenticateAsync(string username, string password);
        UserRole GetCurrentRole();
        Guid? GetCurrentUserId();
    }
}
