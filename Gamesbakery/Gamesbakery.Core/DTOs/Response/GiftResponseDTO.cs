namespace Gamesbakery.Core.DTOs.Response
{
    public class GiftResponseDTO
    {
        public Guid GiftId { get; set; }

        public Guid SenderId { get; set; }

        public Guid RecipientId { get; set; }

        public Guid OrderItemId { get; set; }

        public DateTime GiftDate { get; set; }

        public string GameTitle { get; set; }
    }
}
