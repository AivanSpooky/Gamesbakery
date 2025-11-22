namespace Gamesbakery.Core.DTOs.Response
{
    public class SellerResponseDTO
    {
        public Guid Id { get; set; }
        public string SellerName { get; set; }
        public DateTime RegistrationDate { get; set; }
        public double AvgRating { get; set; }
    }
}
