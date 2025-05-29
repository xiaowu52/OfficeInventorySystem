namespace UserApp.Models
{
    public class RequestViewModel
    {
        public List<Item> AvailableItems { get; set; }
        public List<RequestViewModel> RequestHistory { get; set; }

        // 表单字段
        public string SelectedItemId { get; set; }
        public int Quantity { get; set; }

        // 显示字段
        public string ItemName { get; set; }
        public DateTime RequestDate { get; set; }
        public RequestStatus Status { get; set; }
    }
}
