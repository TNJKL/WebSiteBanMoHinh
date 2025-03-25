using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN ,AUTHOR")]
    public class DashboardController : Controller
    {
        private const int v = 2024;
        private readonly DataContext _dataContext;
        
        public DashboardController(DataContext context)
        {
            _dataContext = context;
           
        }


        public IActionResult Index()
        {
            var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> GetChartData()
        {
            var data = _dataContext.Statisticals
                .Select(s => new
            {
                date = s.CreatedDate.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            })
                .ToList();

            return Json(data);
        }
        [HttpPost]
        public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            var data = _dataContext.Statisticals
                .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
                .Select(s => new
                {
                    date = s.CreatedDate.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data);
        }

        [HttpPost]
        public IActionResult FilterData(DateTime? fromDate, DateTime? toDate)
        {
            var query = _dataContext.Statisticals.AsQueryable();

            if (fromDate.HasValue)
            {
                query = query.Where(s => s.CreatedDate >= fromDate);
            }

            if (toDate.HasValue)
            {
                query = query.Where(s => s.CreatedDate <= toDate);
            }

            var data = query
                .Select(s => new
                {
                    date = s.CreatedDate.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();
            return Json(data);
           
        }

    }
}
