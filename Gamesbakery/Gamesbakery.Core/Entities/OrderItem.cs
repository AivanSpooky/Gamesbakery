namespace Gamesbakery.Core.Entities
{
    public class OrderItem
    {
        public int Id { get; private set; }
        public int OrderId { get; private set; }
        public int GameId { get; private set; }
        public int SellerId { get; private set; }
        public string? Key { get; private set; }

        public OrderItem(int id, int orderId, int gameId, int sellerId, string? key)
        {
            if (orderId <= 0)
                throw new ArgumentException("OrderId must be positive.", nameof(orderId));
            if (gameId <= 0)
                throw new ArgumentException("GameId must be positive.", nameof(gameId));
            if (sellerId <= 0)
                throw new ArgumentException("SellerId must be positive.", nameof(sellerId));
            if (key != null && (string.IsNullOrWhiteSpace(key) || key.Length > 50))
                throw new ArgumentException("Key must be between 1 and 50 characters if provided.", nameof(key));

            Id = id;
            OrderId = orderId;
            GameId = gameId;
            SellerId = sellerId;
            Key = key;
        }

        public void SetKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 50)
                throw new ArgumentException("Key must be between 1 and 50 characters.", nameof(key));
            Key = key;
        }
    }
}