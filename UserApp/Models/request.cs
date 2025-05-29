using System.ComponentModel.DataAnnotations;

namespace UserApp.Models
{
    public class request
    {
        public string item_id { get; set; }
        public int quantity { get; set; }
        public string user_id { get; set; }
        public string user_name { get; set; }

        public DateTime request_date { get; set; }
        public RequestStatus status { get; set; }
    }

    public enum RequestStatus
    {
        [Display(Name = "申请中")]
        申请中,
        [Display(Name = "已通过")]
        已通过,
        [Display(Name = "已驳回")]
        已驳回
    }
}
