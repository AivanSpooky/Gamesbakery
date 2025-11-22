using System;
using System.Collections.Generic;

namespace Gamesbakery.Core.DTOs.OrderDTO
{
    public class OrderCreateDTO
    {
        public List<Guid> CartItemIds { get; set; } = new List<Guid>();
    }
}