using System.Security.Claims;
using Gamesbakery.Core;

namespace Gamesbakery.WebGUI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static Guid? GetUserId(this ClaimsPrincipal user)
        {
            var userIdClaim = user.FindFirst("UserId")?.Value ??
                             user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : null;
        }

        public static Guid? GetSellerId(this ClaimsPrincipal user)
        {
            var sellerIdClaim = user.FindFirst("SellerId")?.Value;
            return string.IsNullOrEmpty(sellerIdClaim) ? null : Guid.Parse(sellerIdClaim);
        }

        public static UserRole GetRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<UserRole>(roleClaim, out var role) ? role : UserRole.Guest;
        }

        public static bool IsAuthenticated(this ClaimsPrincipal user)
        {
            return user.Identity?.IsAuthenticated == true;
        }
    }
}