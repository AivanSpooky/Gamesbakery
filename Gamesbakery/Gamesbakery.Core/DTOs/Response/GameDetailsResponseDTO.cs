namespace Gamesbakery.Core.DTOs.Response
{
    public class GameDetailsResponseDTO
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string Description { get; set; }
        public bool IsForSale { get; set; }
        public string OriginalPublisher { get; set; }
        public decimal AverageRating { get; set; }
    }
}
