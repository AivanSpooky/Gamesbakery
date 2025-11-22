namespace Gamesbakery.WebGUI.Models
{
    public class PaginatedResponse<T>
    {
        public int TotalCount { get; set; }
        public List<T> Items { get; set; }
        public int? NextPage { get; set; }
        public int? PreviousPage { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}
