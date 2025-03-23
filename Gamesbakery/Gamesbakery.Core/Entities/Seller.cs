namespace Gamesbakery.Core.Entities
{
    public class Seller
    {
        public int Id { get; private set; }
        public string SellerName { get; private set; }
        public DateTime RegistrationDate { get; private set; }
        public double AvgRating { get; private set; }

        public Seller(int id, string sellerName, DateTime registrationDate, double avgRating)
        {
            if (string.IsNullOrWhiteSpace(sellerName) || sellerName.Length > 100)
                throw new ArgumentException("SellerName must be between 1 and 100 characters.", nameof(sellerName));
            if (avgRating < 0 || avgRating > 5)
                throw new ArgumentException("AvgRating must be between 0 and 5.", nameof(avgRating));

            Id = id;
            SellerName = sellerName;
            RegistrationDate = registrationDate;
            AvgRating = avgRating;
        }

        public void UpdateRating(double newRating)
        {
            if (newRating < 0 || newRating > 5)
                throw new ArgumentException("AvgRating must be between 0 and 5.", nameof(newRating));
            AvgRating = newRating;
        }
    }
}