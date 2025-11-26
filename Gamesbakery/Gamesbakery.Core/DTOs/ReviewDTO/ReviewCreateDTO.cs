using System.ComponentModel.DataAnnotations;

namespace Gamesbakery.Core.DTOs
{
    public class ReviewCreateDTO
    {
        public Guid GameId { get; set; }

        public string Text { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }
    }
}
