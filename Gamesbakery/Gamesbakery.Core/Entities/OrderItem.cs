namespace Gamesbakery.Core.Entities
{
    public class OrderItem
    {
        public Guid Id { get; private set; }
        public Guid? OrderId { get; private set; }
        public Guid GameId { get; private set; }
        public Guid SellerId { get; private set; }
        public string? Key { get; private set; }
        public bool IsGifted { get; private set; }

        public OrderItem()
        {
        }

        public OrderItem(Guid id, Guid orderId, Guid gameId, Guid sellerId, string? key, bool isGifted = false)
        {
            //if (orderId == Guid.Empty)
            //    throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
            if (gameId == Guid.Empty)
                throw new ArgumentException("GameId cannot be empty.", nameof(gameId));
            if (sellerId == Guid.Empty)
                throw new ArgumentException("SellerId cannot be empty.", nameof(sellerId));
            if (key != null && (string.IsNullOrWhiteSpace(key) || key.Length > 50))
                throw new ArgumentException("Key must be between 1 and 50 characters if provided.", nameof(key));

            Id = id;
            OrderId = orderId;
            GameId = gameId;
            SellerId = sellerId;
            Key = key;
            IsGifted = isGifted;
        }

        public void SetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 50)
                throw new ArgumentException("Key must be between 1 and 50 characters.", nameof(key));
            Key = key;
        }

        public void SetGifted(bool isGifted) => IsGifted = isGifted;
        public void SetOrderId(Guid orderId)
        {
            if (orderId == Guid.Empty)
                throw new ArgumentException("OrderId cannot be empty.", nameof(orderId));
            OrderId = orderId;
        }
    }
}