using System.ComponentModel.DataAnnotations;

namespace WebSiteBanMoHinh.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage ="Vui lòng nhập UserName")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string Email { get; set; }
        [DataType(DataType.Password) ,Required(ErrorMessage ="Vui lòng nhập Password")]
        public string Password { get; set; }
       

    }
}
