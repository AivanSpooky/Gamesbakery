namespace Gamesbakery.Core.Entities
{
    public class Order
    {
        public int Id { get; private set; }
        public int UserId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public decimal Price { get; private set; }
        public bool IsCompleted { get; private set; }
        public bool IsOverdue { get; private set; }

        public Order(int id, int userId, DateTime orderDate, decimal price, bool isCompleted, bool isOverdue)
        {
            if (userId <= 0)
                throw new ArgumentException("UserId must be positive.", nameof(userId));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));

            Id = id;
            UserId = userId;
            OrderDate = orderDate;
            Price = price;
            IsCompleted = isCompleted;
            IsOverdue = isOverdue;
        }

        public void Complete() => IsCompleted = true;
        public void MarkAsOverdue() => IsOverdue = true;
    }
}