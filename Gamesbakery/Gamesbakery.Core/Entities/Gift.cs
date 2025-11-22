using System;

namespace Gamesbakery.Core.Entities
{
    public class Gift
    {
        public Guid Id { get; private set; }
        public Guid SenderId { get; set; }
        public Guid RecipientId { get; set; }
        public Guid OrderItemId { get; set; }
        public DateTime GiftDate { get; set; }
        public GiftSource Type { get; set; } // Sent or Received
        public string GameTitle { get; private set; }
        public string Key { get; private set; }
        public User Sender { get; private set; }
        public User Recipient { get; private set; }
        public OrderItem OrderItem { get; private set; }

        public Gift() { }

        public Gift(Guid id, Guid senderId, Guid recipientId, Guid orderItemId, DateTime giftDate, GiftSource type, string gameTitle, string key)
        {
            if (senderId == Guid.Empty)
                throw new ArgumentException("SenderId cannot be empty.", nameof(senderId));
            if (recipientId == Guid.Empty)
                throw new ArgumentException("RecipientId cannot be empty.", nameof(recipientId));
            if (orderItemId == Guid.Empty)
                throw new ArgumentException("OrderItemId cannot be empty.", nameof(orderItemId));
            if (type == GiftSource.All)
                throw new ArgumentException("Type cannot be 'All'; use 'Sent' or 'Received'.", nameof(type));
            Id = id;
            SenderId = senderId;
            RecipientId = recipientId;
            OrderItemId = orderItemId;
            GiftDate = giftDate;
            Type = type;
            GameTitle = gameTitle;
            Key = key;
        }

        public void Update(GiftSource type, string gameTitle, string key)
        {
            if (type == GiftSource.All)
                throw new ArgumentException("Type cannot be 'All'; use 'Sent' or 'Received'.", nameof(type));
            Type = type;
            GameTitle = gameTitle;
            Key = key;
        }
    }
}