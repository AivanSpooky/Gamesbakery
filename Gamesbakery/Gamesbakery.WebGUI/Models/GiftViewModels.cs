using System.Collections.Generic;
using Gamesbakery.Core.DTOs.GiftDTO;
using Gamesbakery.Core.DTOs.Response;

namespace Gamesbakery.WebGUI.Models
{
    public class GiftIndexViewModel
    {
        public IEnumerable<GiftResponseDTO> SentGifts { get; set; } = new List<GiftResponseDTO>();

        public IEnumerable<GiftResponseDTO> ReceivedGifts { get; set; } = new List<GiftResponseDTO>();
    }

    public class GiftCreateViewModel
    {
        public Guid RecipientId { get; set; }

        public Guid OrderItemId { get; set; }
    }
}
