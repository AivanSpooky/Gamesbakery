using Gamesbakery.Core.DTOs.OrderItemDTO;
namespace Gamesbakery.Core.DTOs.GameDTO
{
    public class GameDetailsDTO // (для детальной информации об игре)
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
        public List<OrderItemDTO.OrderItemDTO> AvailableOrderItems { get; set; } = new List<OrderItemDTO.OrderItemDTO>();
    }
}