using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;

namespace WebSiteBanMoHinh.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel moHinhAnime = new CategoryModel { Name = "Mô hình Anime", Slug = "mohinhanime", Description = "Mô hình các nhân vật anime siêu dễ thương", Status = 1 };
                CategoryModel moHinhSieuNhan = new CategoryModel { Name = "Mô hình siêu nhân", Slug = "mohinhsieunhan", Description = "Mô hình siêu nhân cool ngầu", Status = 1 };

                BrandModel bandai = new BrandModel { Name = "Bandai", Slug = "bandai", Description = "Bandai là một hãng đồ chơi nổi tiếng ở Nhật", Status = 1 };
                BrandModel lego = new BrandModel { Name = "Lego", Slug = "lego", Description = "Lego là hãng đồ chơi lắp ráp nổi tiếng khắp thế giới", Status = 1 };

                _context.Products.AddRange(
                    new ProductModel
                    {
                        Name = "Goku",
                        Slug = "goku",
                        Description = "Goku quá mạnh siêu bá đạo",
                        Image = "1.jpg", // Ảnh đại diện
                        Images = new List<ProductImage> { new ProductImage { Url = "1.jpg" } }, // Ảnh bổ sung
                        Category = moHinhAnime,
                        Brand = bandai,
                        Price = 4500
                    },
                    new ProductModel
                    {
                        Name = "RiderW",
                        Slug = "riderw",
                        Description = "W là hiệp sĩ mặt nạ anh hùng",
                        Image = "1.jpg", // Ảnh đại diện
                        Images = new List<ProductImage> { new ProductImage { Url = "1.jpg" } }, // Ảnh bổ sung
                        Category = moHinhSieuNhan,
                        Brand = lego,
                        Price = 6500
                    }
                );
                _context.SaveChanges();
            }
        }
    }
}
