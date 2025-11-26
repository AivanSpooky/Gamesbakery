namespace Gamesbakery.Core.DTOs.UserDTO
{
    public class UserProfileDTO
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public DateTime RegistrationDate { get; set; }

        public string Country { get; set; }

        public string Password { get; set; }

        public bool IsBlocked { get; set; }

        public decimal Balance { get; set; }

        public decimal TotalSpent { get; set; }
    }
}
