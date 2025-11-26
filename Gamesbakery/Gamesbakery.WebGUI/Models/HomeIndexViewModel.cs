using Gamesbakery.Core.DTOs.Response;
using Gamesbakery.Core.Entities;

namespace Gamesbakery.WebGUI.Models
{
    public class HomeIndexViewModel
    {
        public string Role { get; set; } = string.Empty;

        public IEnumerable<CategoryResponseDTO> Categories { get; set; } = new List<CategoryResponseDTO>();
    }
}
