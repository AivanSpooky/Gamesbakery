using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gamesbakery.Core.DTOs
{
    public class OrderItemGDTO
    {
        public Guid Id { get; set; }
        public Guid GameId { get; set; }
        public string GameTitle { get; set; }
        public decimal GamePrice { get; set; }
        public string Key { get; set; }
    }
}
