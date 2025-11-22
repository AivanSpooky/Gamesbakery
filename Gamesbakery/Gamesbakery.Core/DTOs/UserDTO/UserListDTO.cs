namespace Gamesbakery.Core.DTOs.UserDTO
{
    public class UserListDTO
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsBlocked { get; set; }
    }
}