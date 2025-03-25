using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebSiteBanMoHinh.Repository.Validation;

namespace WebSiteBanMoHinh.Models
{
    public class SliderModel
    {
        
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập tên banner")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Yêu cầu nhập mô tả banner")]
        public string Description { get; set; }

       
        public int? Status { get; set; }
        public string Image { get; set; }
        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }
    }
}
