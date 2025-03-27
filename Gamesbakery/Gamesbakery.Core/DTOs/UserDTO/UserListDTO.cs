namespace Gamesbakery.Core.DTOs.UserDTO
{
    public class UserListDTO // (для списка пользователей)
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}