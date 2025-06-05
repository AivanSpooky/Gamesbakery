namespace Gamesbakery.Core.Entities
{
    public class Gift
    {
        public Guid Id { get; private set; }
        public Guid SenderId { get; private set; }
        public Guid RecipientId { get; private set; }
        public Guid OrderItemId { get; private set; }
        public DateTime GiftDate { get; private set; }
        public OrderItem OrderItem { get; set; }

        public Gift()
        {
        }

        public Gift(Guid id, Guid senderId, Guid recipientId, Guid orderItemId, DateTime giftDate)
        {
            if (senderId == Guid.Empty)
                throw new ArgumentException("SenderId cannot be empty.", nameof(senderId));
            if (recipientId == Guid.Empty)
                throw new ArgumentException("RecipientId cannot be empty.", nameof(recipientId));
            if (orderItemId == Guid.Empty)
                throw new ArgumentException("OrderItemId cannot be empty.", nameof(orderItemId));

            Id = id;
            SenderId = senderId;
            RecipientId = recipientId;
            OrderItemId = orderItemId;
            GiftDate = giftDate;
        }
    }
}