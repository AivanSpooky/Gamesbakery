using System.Text.RegularExpressions;

namespace Gamesbakery.Core.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; private set; }
        public string Email { get; private set; }
        public DateTime RegistrationDate { get; private set; }
        public string Country { get; private set; }
        public string Password { get; private set; }
        public bool IsBlocked { get; private set; }
        public decimal Balance { get; private set; }

        public User()
        {
        }

        public User(Guid id, string username, string email, DateTime registrationDate, string country, string password, bool isBlocked, decimal balance)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
                throw new ArgumentException("Username must be between 1 and 50 characters.", nameof(username));
            if (string.IsNullOrWhiteSpace(email) || email.Length > 100 || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new ArgumentException("Invalid email format.", nameof(email));
            if (string.IsNullOrWhiteSpace(country) || country.Length > 300 || !CountryProvider.IsValidCountry(country))
                throw new ArgumentException("Country must be a valid country name (max 300 characters, no spaces).", nameof(country));
            if (string.IsNullOrWhiteSpace(password) || password.Length > 100)
                throw new ArgumentException("Password must be between 1 and 100 characters.", nameof(password));
            if (balance < 0)
                throw new ArgumentException("Balance cannot be negative.", nameof(balance));

            Id = id;
            Username = username;
            Email = email;
            RegistrationDate = registrationDate;
            Country = country;
            Password = password;
            IsBlocked = isBlocked;
            Balance = balance;
        }

        public void UpdateBalance(decimal newBalance)
        {
            if (newBalance < 0)
                throw new ArgumentException("Balance cannot be negative.", nameof(newBalance));
            Balance = newBalance;
        }
        public void SetBlock(bool b) => IsBlocked = b ? true : false;
        public void Block() => IsBlocked = true;
        public void Unblock() => IsBlocked = false;
    }
}