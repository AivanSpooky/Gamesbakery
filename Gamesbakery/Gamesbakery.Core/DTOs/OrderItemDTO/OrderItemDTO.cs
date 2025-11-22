using System;

namespace Gamesbakery.Core.DTOs.OrderItemDTO
{
    public class OrderItemDTO
    {
        public Guid Id { get; set; }
        public Guid? OrderId { get; set; }
        public Guid GameId { get; set; }
        public Guid SellerId { get; set; }
        public string? Key { get; set; }
        public string GameTitle { get; set; }
        public string SellerName { get; set; }
        public decimal GamePrice { get; set; }
        public bool IsGifted { get; set; }
    }
}