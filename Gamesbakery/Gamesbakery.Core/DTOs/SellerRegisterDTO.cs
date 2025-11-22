using System.ComponentModel.DataAnnotations;

namespace Gamesbakery.Core.DTOs
{
    public class SellerRegisterDTO
    {
        public string SellerName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}