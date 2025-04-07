using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Controllers
{
    
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;
        }

        //public async Task<IActionResult> Index(string Slug = "")
        //{
        //    BrandModel brand = _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();
        //    if (brand == null) return RedirectToAction("Index");
        //    var productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
        //    return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
        //}

        public async Task<IActionResult> Index(string Slug = "", string sort_by = "", string startprice = "", string endprice = "")
        {
            BrandModel brand = _dataContext.Brands.Where(c => c.Slug == Slug).FirstOrDefault();
            if (brand == null) return RedirectToAction("Index");
            ViewBag.Slug = Slug;
            IQueryable<ProductModel> productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);
            var count = await productsByBrand.CountAsync();
            if (count > 0)
            {
                if (sort_by == "price_increase")
                {
                    productsByBrand = productsByBrand.OrderBy(p => p.Price);
                }
                else if (sort_by == "price_decrease")
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Price);
                }
                else if (sort_by == "price_newest")
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                }
                else if (sort_by == "price_oldest")
                {
                    productsByBrand = productsByBrand.OrderBy(p => p.Id);
                }
                else if (startprice != "" && endprice != "")
                {
                    double startPriceValue;
                    double endPriceValue;

                    if (double.TryParse(startprice, out startPriceValue) && double.TryParse(endprice, out endPriceValue))
                    {
                        productsByBrand = productsByBrand.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
                    }
                    else
                    {
                        productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                    }
                }
                else
                {
                    productsByBrand = productsByBrand.OrderByDescending(p => p.Id);
                }
            }
            return View(await productsByBrand.ToListAsync());
        }
    }
}
