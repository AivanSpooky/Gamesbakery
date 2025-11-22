using System;
using Gamesbakery.Core;

namespace Gamesbakery.Core.DTOs.GiftDTO
{
    public class GiftCreateDTO
    {
        public Guid RecipientId { get; set; }
        public Guid OrderItemId { get; set; }
    }
}