using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Models.ViewModels;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Controllers
{
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        public ProductController(DataContext context)
        {
            _dataContext = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        //     public async Task<IActionResult> Details(int Id)
        //     {
        //         if (Id == null) return RedirectToAction("Index");
        //         var productsById = _dataContext.Products
        //             .Include(p =>p.Ratings)
        //             .Where(p => p.Id == Id).FirstOrDefault();
        //         var relatedProducts = await _dataContext.Products
        //.Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
        //.Take(4)
        //.ToListAsync();

        //ViewBag.RelatedProducts = relatedProducts;
        //         var viewModel = new ProductDetailsViewModel
        //         {
        //             ProductDetails = productsById,

        //         };
        //         return View(viewModel);
        //     }

        public async Task<IActionResult> Details(int Id, int page = 1)
        {
            if (Id == null) return RedirectToAction("Index");

            // Lấy sản phẩm theo Id, bao gồm Ratings
            var productsById = _dataContext.Products
                .Include(p => p.Ratings)
                .Where(p => p.Id == Id)
                .FirstOrDefault();

            if (productsById == null) return NotFound(); // Kiểm tra nếu không tìm thấy sản phẩm

            // Lấy sản phẩm liên quan
            var relatedProducts = await _dataContext.Products
                .Where(p => p.CategoryId == productsById.CategoryId && p.Id != productsById.Id)
                .Take(4)
                .ToListAsync();

            // Thiết lập phân trang cho Ratings
            int pageSize = 5; // 5 bình luận mỗi trang
            int totalReviews = productsById.Ratings?.Count() ?? 0;
            int totalPages = (int)Math.Ceiling(totalReviews / (double)pageSize);

            // Lấy danh sách Ratings cho trang hiện tại
            var paginatedRatings = productsById.Ratings
                ?.Skip((page - 1) * pageSize)
                ?.Take(pageSize)
                ?.ToList();

            // Tạo view model
            var viewModel = new ProductDetailsViewModel
            {
                ProductDetails = productsById,
                // Gán danh sách Ratings đã phân trang
                // Nếu Ratings là một thuộc tính trong ProductDetails, cần đảm bảo chỉ gán danh sách đã phân trang
            };

            // Gán Ratings đã phân trang vào ProductDetails (nếu Ratings là thuộc tính của Product)
            productsById.Ratings = paginatedRatings;

            // Truyền dữ liệu qua ViewBag
            ViewBag.RelatedProducts = relatedProducts;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.ProductId = Id; // Truyền Id để sử dụng trong liên kết phân trang
            ViewBag.TotalReviews = totalReviews;
            return View(viewModel);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]

        //public async Task<IActionResult> CommentProduct(RatingModel rating)
        //{
        //    if (ModelState.IsValid)
        //    {

        //        var ratingEntity = new RatingModel
        //        {
        //            ProductId = rating.ProductId,
        //            Name = rating.Name,
        //            Email = rating.Email,
        //            Comment = rating.Comment,
        //            Star = rating.Star

        //        };

        //        _dataContext.Ratings.Add(ratingEntity);
        //        await _dataContext.SaveChangesAsync();

        //        TempData["success"] = "Thêm đánh giá thành công";

        //        return Redirect(Request.Headers["Referer"]);
        //    }
        //    else
        //    {
        //        TempData["error"] = "Model có một vài thứ đang lỗi";
        //        List<string> errors = new List<string>();
        //        foreach (var value in ModelState.Values)
        //        {
        //            foreach (var error in value.Errors)
        //            {
        //                errors.Add(error.ErrorMessage);
        //            }
        //        }
        //        string errorMessage = string.Join("\n", errors);

        //        return RedirectToAction("Detail", new { id = rating.ProductId });
        //    }

        //    return Redirect(Request.Headers["Referer"]);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> CommentProduct(RatingModel rating)
        {
            if (ModelState.IsValid)
            {

                var ratingEntity = new RatingModel
                {
                    ProductId = rating.ProductId,
                    Name = rating.Name,
                    Email = rating.Email,
                    Comment = rating.Comment,
                    Star = rating.Star

                };

                _dataContext.Ratings.Add(ratingEntity);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Thêm đánh giá thành công";

                return Redirect(Request.Headers["Referer"]);
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);

                return RedirectToAction("Detail", new { id = rating.ProductId });
            }

            return Redirect(Request.Headers["Referer"]);
        }

        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _dataContext.Products
            .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
            .ToListAsync();

            ViewBag.Keyword = searchTerm;

            return View(products);
        }
        


        
    }
}
