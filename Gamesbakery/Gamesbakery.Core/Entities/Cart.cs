using System;
using System.Collections.Generic;

namespace Gamesbakery.Core.Entities
{
    public class Cart
    {
        public Guid CartId { get; private set; }
        public Guid UserId { get; set; }
        public List<CartItem> Items { get; private set; } = new List<CartItem>();
        public User User { get; private set; } // Added

        public Cart()
        {
        }

        public Cart(Guid cartId, Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            CartId = cartId;
            UserId = userId;
        }

        public void AddItem(CartItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            Items.Add(item);
        }

        public void RemoveItem(Guid orderItemId)
        {
            var item = Items.FirstOrDefault(i => i.OrderItemID == orderItemId);
            if (item != null)
                Items.Remove(item);
        }
    }
}