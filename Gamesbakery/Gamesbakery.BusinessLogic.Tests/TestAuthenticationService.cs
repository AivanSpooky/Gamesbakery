using Gamesbakery.Core;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Tests
{
    public class TestAuthenticationService : IAuthenticationService
    {
        public UserRole GetCurrentRole() => UserRole.Admin;
        public Guid? GetCurrentUserId() => null;
        public Guid? GetCurrentSellerId() => null;
        public Task<(UserRole Role, Guid? UserId, Guid? SellerId)> AuthenticateAsync(string username, string password) =>
            Task.FromResult<(UserRole, Guid?, Guid?)>((UserRole.Guest, null, null));

        public Task<User> RegisterUserAsync(string username, string email, string password, string country)
        {
            // Простая реализация для тестов, возвращает null в случае ошибки
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(country))
                return Task.FromResult<User>(null);
            var user = new User(Guid.NewGuid(), username, email, DateTime.UtcNow, country, password, false, 0m);
            return Task.FromResult(user);
        }
    }
}
