using System;

namespace Gamesbakery.Core.DTOs.CartDTO
{
    public class CartItemDTO
    {
        public Guid OrderItemId { get; set; }
        public Guid GameId { get; set; }
        public string GameTitle { get; set; }
        public decimal GamePrice { get; set; }
        public string? Key { get; set; }
        public string SellerName { get; set; }
        public Guid SellerId { get; set; }
    }
}