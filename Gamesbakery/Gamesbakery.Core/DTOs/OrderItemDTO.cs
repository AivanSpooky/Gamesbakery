namespace Gamesbakery.Core.DTOs
{
    public class OrderItemDTO
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid GameId { get; set; }
        public Guid SellerId { get; set; }
        public string? Key { get; set; }
    }
}