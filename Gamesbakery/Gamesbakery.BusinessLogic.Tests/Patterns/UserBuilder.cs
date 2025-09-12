using Gamesbakery.Core.Entities;

namespace Gamesbakery.BusinessLogic.Tests.Patterns
{
    public class UserBuilder
    {
        private Guid id = Guid.NewGuid();
        private string username = "TestUser";
        private string email = "test@example.com";
        private DateTime registrationDate = DateTime.UtcNow;
        private string country = "United States";
        private string password = "password123";
        private bool isBlocked = false;
        private decimal balance = 100;

        public UserBuilder WithId(Guid newId)
        {
            id = newId;
            return this;
        }

        public UserBuilder WithUsername(string newUsername)
        {
            username = newUsername;
            return this;
        }

        public UserBuilder WithEmail(string newEmail)
        {
            email = newEmail;
            return this;
        }

        public UserBuilder WithRegistrationDate(DateTime newDate)
        {
            registrationDate = newDate;
            return this;
        }

        public UserBuilder WithCountry(string newCountry)
        {
            country = newCountry;
            return this;
        }

        public UserBuilder WithPassword(string newPassword)
        {
            password = newPassword;
            return this;
        }

        public UserBuilder Blocked(bool blocked)
        {
            isBlocked = blocked;
            return this;
        }

        public UserBuilder WithBalance(decimal newBalance)
        {
            balance = newBalance;
            return this;
        }

        public User Build()
        {
            return new User(id, username, email, registrationDate, country, password, isBlocked, balance);
        }
    }
}
