namespace Gamesbakery.Core.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public DateTime OrderDate { get; private set; }
        public void SetOrderDate(DateTime od) => OrderDate = od;
        public decimal Price { get; private set; }
        public void SetPrice(decimal p) => Price = p;
        public bool IsCompleted { get; private set; }
        public bool IsOverdue { get; private set; }
        public void SetComplete(bool c) => IsCompleted = c ? true : false;
        public void SetOverdue(bool o) => IsOverdue = o ? true : false;
        public void Complete() => IsCompleted = true;
        public void MarkAsOverdue() => IsOverdue = true;

        public Order()
        {
        }

        public Order(Guid id, Guid userId, DateTime orderDate, decimal price, bool isCompleted, bool isOverdue)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(price));

            Id = id;
            UserId = userId;
            OrderDate = orderDate;
            Price = price;
            IsCompleted = isCompleted;
            IsOverdue = isOverdue;
        }
    }
}