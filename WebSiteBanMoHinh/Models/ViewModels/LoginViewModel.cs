using System.ComponentModel.DataAnnotations;

namespace WebSiteBanMoHinh.Models.ViewModels
{
    public class LoginViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập UserName")]
        public string UserName { get; set; }
       
        [DataType(DataType.Password), Required(ErrorMessage = "Vui lòng nhập Password")]
        public string Password { get; set; }
        public string ReturnUrl { get; set; }
    }
}
