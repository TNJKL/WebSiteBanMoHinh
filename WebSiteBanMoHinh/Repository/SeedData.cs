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
                //CategoryModel macbook = new CategoryModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is Large product in the world", Status = 1 };
                //CategoryModel pc = new CategoryModel { Name = "Pc", Slug = "pc", Description = "Pc is Large Product in the world", Status = 1 };

                //BrandModel apple = new BrandModel { Name = "Apple", Slug = "apple", Description = "Apple is Large Brand in the world", Status = 1 };
                //BrandModel samsung = new BrandModel { Name = "Samsung", Slug = "samsung", Description = "Samsung is Large Brand in the world", Status = 1 };
                //_context.Products.AddRange(
                //    new ProductModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is Best", Image = "1.jpg", Category = macbook, Brand = apple, Price = 1233, },
                //    new ProductModel { Name = "Pc", Slug = "pc", Description = "Pc is Best", Image = "1.jpg", Category = pc, Brand = samsung, Price = 1233, }

                CategoryModel moHinhAnime = new CategoryModel { Name = "Mô hình Anime", Slug = "mohinhanime", Description = "Mô hình các nhân vật anime siêu dễ thương", Status = 1 };
                CategoryModel moHinhSieuNhan = new CategoryModel { Name = "Mô hình siêu nhân", Slug = " mohinhsieunhan", Description = "Mô hình siêu nhân cool ngầu", Status = 1 };

                BrandModel bandai = new BrandModel { Name = "Bandai", Slug = "bandai", Description = "Bandai là một hãng đồ chơi nổi tiếng ở Nhật", Status = 1 };
                BrandModel lego = new BrandModel { Name = "Lego", Slug = "lego", Description = "Lego là hãng đồ chơi lắp ráp nổi tiếng khắp thế giới", Status = 1 };
                _context.Products.AddRange(
                    new ProductModel { Name = "Goku", Slug = "goku", Description = "Goku quá mạnh siêu bá đạo", Image = "1.jpg", Category = moHinhAnime, Brand = bandai, Price = 4500, },
                    new ProductModel { Name = "RiderW", Slug = "riderw", Description = "W là hiệp sĩ mặt nạ anh hùng", Image = "1.jpg", Category = moHinhSieuNhan, Brand = lego, Price = 6500, }
                );
                _context.SaveChanges();
            }
        }
    }
}
