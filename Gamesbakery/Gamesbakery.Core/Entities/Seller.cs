namespace Gamesbakery.Core.Entities
{
    public class Seller
    {
        public Guid Id { get; private set; }
        public string SellerName { get; private set; }
        public void SetSellerName(string sellerName) => SellerName = sellerName;
        public DateTime RegistrationDate { get; private set; }
        public void SetRegistrationDate(DateTime rd) => RegistrationDate = rd;
        public double AvgRating { get; private set; }
        public void SetAvgRating(double ar) => AvgRating = ar;
        public string Password { get; private set; }
        public void SetPassword(string password) => Password = password;

        public Seller()
        {
        }

        public Seller(Guid id, string sellerName, DateTime registrationDate, double avgRating, string password)
        {
            if (string.IsNullOrWhiteSpace(sellerName) || sellerName.Length > 100)
                throw new ArgumentException("SellerName must be between 1 and 100 characters.", nameof(sellerName));
            if (avgRating < 0 || avgRating > 5)
                throw new ArgumentException("AvgRating must be between 0 and 5.", nameof(avgRating));
            if (string.IsNullOrWhiteSpace(password) || password.Length > 100)
                throw new ArgumentException("Password must be between 1 and 100 characters.", nameof(password));

            Id = id;
            SellerName = sellerName;
            RegistrationDate = registrationDate;
            AvgRating = avgRating;
            Password = password;
        }

        public void UpdateRating(double newRating)
        {
            if (newRating < 0 || newRating > 5)
                throw new ArgumentException("AvgRating must be between 0 and 5.", nameof(newRating));
            AvgRating = newRating;
        }
    }
}