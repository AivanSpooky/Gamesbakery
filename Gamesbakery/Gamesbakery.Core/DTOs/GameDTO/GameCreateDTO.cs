using System.ComponentModel.DataAnnotations;

namespace Gamesbakery.Core.DTOs.GameDTO
{
    public class GameCreateDTO
    {
        public Guid CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public DateTime ReleaseDate { get; set; }

        public string Description { get; set; } = string.Empty;

        public string OriginalPublisher { get; set; } = string.Empty;

        public bool IsForSale { get; set; } = true;
    }
}
