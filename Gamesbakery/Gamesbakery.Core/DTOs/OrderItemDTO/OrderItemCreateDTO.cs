namespace Gamesbakery.Core.DTOs.OrderItemDTO
{
    public class OrderItemCreateDTO
    {
        public Guid GameId { get; set; }

        public Guid SellerId { get; set; }

        public string? Key { get; set; }
    }
}
