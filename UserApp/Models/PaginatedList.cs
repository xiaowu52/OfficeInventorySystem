namespace UserApp.Models
{
    public class PaginatedList<T> where T : class
    {
        public List<T> Items { get; set; } = new List<T>();
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;
    }

    public class RequestItemPageViewModel
    {
        public List<Item> AvailableItems { get; set; } = new List<Item>();
        public PaginatedList<RequestViewModel> RequestHistory { get; set; } = new PaginatedList<RequestViewModel>();
        public RequestViewModel NewRequest { get; set; } = new RequestViewModel();
    }
}
