public class UserProfileDTO
{
    public Guid Id { get; set; } // (для профиля пользователя)
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime RegistrationDate { get; set; }
    public string Country { get; set; }
    public bool IsBlocked { get; set; }
    public decimal Balance { get; set; }
}