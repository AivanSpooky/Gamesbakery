namespace Gamesbakery.Core.DTOs.Response
{
    public class GameListResponseDTO
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public decimal Price { get; set; }

        public bool IsForSale { get; set; }
    }
}
