using Gamesbakery.Core;
using Gamesbakery.Core.Entities;
using Gamesbakery.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.BusinessLogic.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly GamesbakeryDbContext _context;
        private UserRole _currentRole;
        private Guid? _currentUserId;
        private Guid? _currentSellerId;

        public AuthenticationService(GamesbakeryDbContext context)
        {
            _context = context;
        }

        public async Task<(UserRole Role, Guid? UserId, Guid? SellerId)> AuthenticateAsync(string username, string password)
        {
            if (username == "AdminUser" && password == "AdminPass123")
            {
                _currentRole = UserRole.Admin;
                _currentUserId = null;
                _currentSellerId = null;
                return (_currentRole, _currentUserId, _currentSellerId);
            }
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                if (username == "GuestUser")
                {
                    _currentRole = UserRole.Guest;
                    _currentUserId = null;
                    _currentSellerId = null;
                }
                else
                {
                    _currentRole = UserRole.User;
                    _currentUserId = user.Id;
                    _currentSellerId = null;
                }
                return (_currentRole, _currentUserId, _currentSellerId);
            }
            
            var seller = await _context.Sellers
                .FirstOrDefaultAsync(s => s.SellerName == username && s.Password == password);
            if (seller != null)
            {
                _currentRole = UserRole.Seller;
                _currentUserId = null;
                _currentSellerId = seller.Id;
                return (_currentRole, _currentUserId, _currentSellerId);
            }

            _currentRole = UserRole.Guest;
            _currentUserId = null;
            _currentSellerId = null;
            return (_currentRole, _currentUserId, _currentSellerId);
        }

        public UserRole GetCurrentRole()
        {
            return _currentRole;
        }

        public Guid? GetCurrentUserId()
        {
            return _currentUserId;
        }

        public Guid? GetCurrentSellerId()
        {
            return _currentSellerId;
        }
    }
}