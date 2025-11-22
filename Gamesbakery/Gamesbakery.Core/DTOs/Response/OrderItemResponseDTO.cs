namespace Gamesbakery.Core.DTOs.Response
{
    public class OrderItemResponseDTO
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string GameTitle { get; set; }
        public Guid SellerId { get; set; }
        public string SellerName { get; set; }
        public Guid? OrderId { get; set; } // Added for status determination
        public decimal GamePrice { get; set; } // Added for price display
    }
}
