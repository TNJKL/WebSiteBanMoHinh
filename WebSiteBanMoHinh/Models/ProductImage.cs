namespace WebSiteBanMoHinh.Models
{
    public class ProductImage
    {
        public long Id { get; set; } // Sử dụng long để đồng bộ với Id của ProductModel
        public string Url { get; set; } // Đường dẫn của ảnh
        public long ProductId { get; set; } // Khóa ngoại liên kết với Product
        public virtual ProductModel Product { get; set; } // Navigatio
    }
}
