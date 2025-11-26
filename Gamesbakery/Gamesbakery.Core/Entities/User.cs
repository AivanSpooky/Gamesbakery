using System;
using System.Collections.Generic;
using System.Globalization;
using Gamesbakery.Core;

namespace Gamesbakery.Core.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime RegistrationDate { get; private set; }
        public string Country { get; set; }
        public string Password { get; private set; }
        public bool IsBlocked { get; set; }
        public decimal Balance { get; private set; }
        public decimal TotalSpent { get; private set; }
        public Cart Cart { get; private set; } // Added
        public List<Order> Orders { get; private set; } // Added
        public List<Review> Reviews { get; private set; } // Added
        public List<Gift> SentGifts { get; private set; } // Added
        public List<Gift> ReceivedGifts { get; private set; } // Added

        public User()
        {
            Orders = new List<Order>();
            Reviews = new List<Review>();
            SentGifts = new List<Gift>();
            ReceivedGifts = new List<Gift>();
        }

        public User(Guid id, string username, string email, DateTime registrationDate, string country, string password, bool isBlocked, decimal balance)
        {
            ValidateConstructorParameters(username, email, country, password, balance);
            Id = id;
            Username = username;
            Email = email;
            RegistrationDate = registrationDate;
            Country = country;
            Password = password;
            IsBlocked = isBlocked;
            Balance = balance;
            TotalSpent = 0;
            Orders = new List<Order>();
            Reviews = new List<Review>();
            SentGifts = new List<Gift>();
            ReceivedGifts = new List<Gift>();
        }

        private void ValidateConstructorParameters(string username, string email, string country, string password, decimal balance)
        {
            ValidateString(username, 1, 50, nameof(username));
            ValidateString(email, 1, 100, nameof(email));
            ValidateCountry(country);
            ValidateString(password, 1, 100, nameof(password));
            ValidateBalance(balance);
        }

        private void ValidateString(string value, int minLength, int maxLength, string paramName)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length > maxLength)
                throw new ArgumentException($"{paramName} must be between {minLength} and {maxLength} characters.", paramName);
        }

        private void ValidateCountry(string country)
        {
            if (string.IsNullOrWhiteSpace(country) || country.Length > 300 || !CountryProvider.IsValidCountry(country))
                throw new ArgumentException("Country must be a valid name between 1 and 300 characters.", nameof(country));
        }

        private void ValidateBalance(decimal balance)
        {
            if (balance < 0)
                throw new ArgumentException("Balance cannot be negative.", nameof(balance));
        }

        public void UpdateBalance(decimal newBalance)
        {
            if (newBalance < 0)
                throw new ArgumentException("Balance cannot be negative.", nameof(newBalance));
            Balance = newBalance;
        }

        public void UpdateCountry(string newCountry)
        {
            if (string.IsNullOrWhiteSpace(newCountry) || newCountry.Length > 300)
                throw new ArgumentException("Country must be between 1 and 300 characters.", nameof(newCountry));
            if (!CountryProvider.IsValidCountry(newCountry))
                throw new ArgumentException($"Country '{newCountry}' is not a valid country name.", nameof(newCountry));
            Country = newCountry;
        }

        public void Block() => IsBlocked = true;
        public void Unblock() => IsBlocked = false;
        public void UpdateTotalSpent(decimal totalSpent) => TotalSpent = totalSpent;
    }
}
