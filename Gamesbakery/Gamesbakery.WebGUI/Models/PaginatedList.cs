using Microsoft.EntityFrameworkCore;

namespace Gamesbakery.WebGUI.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }

        public int TotalCount { get; }

        public int Page { get; }

        public int PageSize { get; }

        public int TotalPages => (int)Math.Ceiling((double)this.TotalCount / this.PageSize);

        public bool HasNext => this.Page < this.TotalPages;

        public bool HasPrevious => this.Page > 1;

        public PaginatedList(List<T> items, int totalCount, int page, int pageSize)
        {
            this.Items = items;
            this.TotalCount = totalCount;
            this.Page = page;
            this.PageSize = pageSize;
        }
    }
}
