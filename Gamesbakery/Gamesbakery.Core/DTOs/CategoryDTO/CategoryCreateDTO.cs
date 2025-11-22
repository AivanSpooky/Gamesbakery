using System.ComponentModel.DataAnnotations;

namespace Gamesbakery.Core.DTOs
{
    public class CategoryCreateDTO
    {
        public string GenreName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}