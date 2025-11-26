using System;
using Gamesbakery.Core;

namespace Gamesbakery.Core.DTOs.GiftDTO
{
    public class GiftDTO
    {
        public Guid GiftId { get; set; }

        public Guid SenderId { get; set; }

        public Guid RecipientId { get; set; }

        public Guid OrderItemId { get; set; }

        public DateTime GiftDate { get; set; }

        public GiftSource Type { get; set; } // Enum

        public string GameTitle { get; set; }

        public string Key { get; set; }
    }
}
