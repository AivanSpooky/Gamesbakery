using System;
using System.Collections.Generic;

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

        public List<CartItem> CartItems { get; private set; } // Added

        public Seller Seller { get; private set; } // Added

        public Game Game { get; private set; } // Added

        public OrderItem()
        {
            this.CartItems = new List<CartItem>();
        }

        public OrderItem(Guid id, Guid? orderId, Guid gameId, Guid sellerId, string? key, bool isGifted)
        {
            if (gameId == Guid.Empty)
                throw new ArgumentException("GameId cannot be empty.", nameof(gameId));
            if (sellerId == Guid.Empty)
                throw new ArgumentException("SellerId cannot be empty.", nameof(sellerId));
            this.Id = id;
            this.OrderId = orderId;
            this.GameId = gameId;
            this.SellerId = sellerId;
            this.Key = key;
            this.IsGifted = isGifted;
            this.CartItems = new List<CartItem>();
        }

        public void SetOrderId(Guid? orderId)
        {
            this.OrderId = orderId;
        }

        public void SetKey(string? key)
        {
            this.Key = key;
        }

        public void SetGifted(bool isGifted)
        {
            this.IsGifted = isGifted;
        }
    }
}
