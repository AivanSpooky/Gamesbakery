using System;

namespace Gamesbakery.Core.Entities
{
    public class CartItem
    {
        public Guid CartItemID { get; private set; }

        public Guid CartID { get; private set; }

        public Guid OrderItemID { get; private set; }

        public virtual Cart Cart { get; private set; } // Исправлено с CartItem на Cart

        public virtual OrderItem OrderItem { get; private set; }

        public CartItem()
        {
        }

        public CartItem(Guid cartItemID, Guid cartID, Guid orderItemID)
        {
            if (cartID == Guid.Empty)
                throw new ArgumentException("CartID cannot be empty.", nameof(cartID));
            if (orderItemID == Guid.Empty)
                throw new ArgumentException("OrderItemID cannot be empty.", nameof(orderItemID));
            this.CartItemID = cartItemID;
            this.CartID = cartID;
            this.OrderItemID = orderItemID;
        }
    }
}
