namespace Gamesbakery.Core.DTOs
{
    public class SellerDTO
    {
        public Guid Id { get; set; }

        public string SellerName { get; set; }

        public DateTime RegistrationDate { get; set; }

        public double AvgRating { get; set; }

        public string Password { get; set; }
    }
}
