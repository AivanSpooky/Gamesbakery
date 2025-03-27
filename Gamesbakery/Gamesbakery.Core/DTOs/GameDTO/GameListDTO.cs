namespace Gamesbakery.Core.DTOs.GameDTO
{
    public class GameListDTO // (для списка игр)
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public bool IsForSale { get; set; }
    }
}