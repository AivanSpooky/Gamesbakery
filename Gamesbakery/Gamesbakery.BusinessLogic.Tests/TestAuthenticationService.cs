using Gamesbakery.Core;

namespace Gamesbakery.BusinessLogic.Tests
{
    public class TestAuthenticationService : IAuthenticationService
    {
        public UserRole GetCurrentRole() => UserRole.Admin;
        public Guid? GetCurrentUserId() => null;
        public Guid? GetCurrentSellerId() => null;
        public Task<(UserRole Role, Guid? UserId, Guid? SellerId)> AuthenticateAsync(string username, string password) =>
            Task.FromResult<(UserRole, Guid?, Guid?)>((UserRole.Guest, null, null));
    }
}
