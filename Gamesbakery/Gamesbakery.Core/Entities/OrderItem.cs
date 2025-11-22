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
            CartItems = new List<CartItem>();
        }

        public OrderItem(Guid id, Guid? orderId, Guid gameId, Guid sellerId, string? key, bool isGifted)
        {
            if (gameId == Guid.Empty)
                throw new ArgumentException("GameId cannot be empty.", nameof(gameId));
            if (sellerId == Guid.Empty)
                throw new ArgumentException("SellerId cannot be empty.", nameof(sellerId));
            Id = id;
            OrderId = orderId;
            GameId = gameId;
            SellerId = sellerId;
            Key = key;
            IsGifted = isGifted;
            CartItems = new List<CartItem>();
        }

        public void SetOrderId(Guid? orderId)
        {
            OrderId = orderId;
        }

        public void SetKey(string? key)
        {
            Key = key;
        }

        public void SetGifted(bool isGifted)
        {
            IsGifted = isGifted;
        }
    }
}