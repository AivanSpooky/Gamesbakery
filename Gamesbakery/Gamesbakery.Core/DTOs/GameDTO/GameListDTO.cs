namespace Gamesbakery.Core.DTOs.GameDTO
{
    public class GameListDTO
    {
        public Guid Id { get; set; }
        public Guid CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsForSale { get; set; }
    }
}