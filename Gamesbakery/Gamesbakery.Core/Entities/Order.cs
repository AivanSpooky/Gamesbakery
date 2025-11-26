using System;
using System.Collections.Generic;

namespace Gamesbakery.Core.Entities
{
    public class Order
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public DateTime OrderDate { get; private set; }

        public decimal TotalAmount { get; private set; }

        public string Status { get; private set; }

        public bool IsCompleted { get; private set; }

        public bool IsOverdue { get; private set; }

        public List<OrderItem> OrderItems { get; private set; } = new List<OrderItem>();

        public User User { get; private set; } // Added

        public Order()
        {
        }

        public Order(Guid id, Guid userId, DateTime orderDate, decimal totalAmount, string status, bool isCompleted, bool isOverdue)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (totalAmount < 0)
                throw new ArgumentException("TotalAmount cannot be negative.", nameof(totalAmount));
            if (string.IsNullOrWhiteSpace(status))
                throw new ArgumentException("Status cannot be empty.", nameof(status));
            this.Id = id;
            this.UserId = userId;
            this.OrderDate = orderDate;
            this.TotalAmount = totalAmount;
            this.Status = status;
            this.IsCompleted = isCompleted;
            this.IsOverdue = isOverdue;
        }

        public void Complete()
        {
            this.Status = "Completed";
            this.IsCompleted = true;
        }

        public void MarkAsOverdue() => this.IsOverdue = true;

        public void AddOrderItem(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            this.OrderItems.Add(item);
            item.SetOrderId(this.Id);
        }

        public void UpdateTotalAmount(decimal newTotal)
        {
            if (newTotal < 0)
                throw new ArgumentException("TotalAmount cannot be negative.", nameof(newTotal));
            this.TotalAmount = newTotal;
        }

        public void UpdateStatus(string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status cannot be empty.", nameof(newStatus));
            this.Status = newStatus;
        }
    }
}
