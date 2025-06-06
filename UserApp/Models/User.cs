using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserApp.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public string UserID { get; set; }

        [Column("password")]
        public string Password { get; set; }

        [Column("user_name")]
        public string UserName { get; set; }

        [Column("gender")]
        public string Gender { get; set; }

        [Column("birth_date")]
        public DateTime BirthDate { get; set; }

        [Column("phone_number")]
        public string PhoneNumber { get; set; }
    }
}
