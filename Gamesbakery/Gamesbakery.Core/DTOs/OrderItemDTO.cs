namespace Gamesbakery.Core.DTOs
{
    public class OrderItemDTO
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int GameId { get; set; }
        public int SellerId { get; set; }
        public string? Key { get; set; }
    }
}