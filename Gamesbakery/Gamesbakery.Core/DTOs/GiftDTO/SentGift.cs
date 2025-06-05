namespace Gamesbakery.Core.DTOs.GiftDTO
{
    public class SentGift
    {
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public Guid OrderItemId { get; set; }
        public DateTime GiftDate { get; set; }
    }
}