using System.ComponentModel.DataAnnotations;

namespace Gamesbakery.Core.DTOs
{
    public class CategoryUpdateDTO
    {
        public string GenreName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}