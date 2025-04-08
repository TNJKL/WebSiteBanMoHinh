using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Identity.Client;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;



namespace WebSiteBanMoHinh.Controllers;

public class HomeController : Controller
{   //localizer

   
    private readonly DataContext _dataContext;
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<AppUserModel> _userManager;

    public HomeController(ILogger<HomeController> logger , DataContext context , UserManager<AppUserModel> userManager)
    {
        _logger = logger;
        _dataContext = context;
        _userManager = userManager;
    }
    public IActionResult ChangeLang(string culture)
    {
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)), new CookieOptions()
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });
        return Redirect(Request.Headers["Referer"].ToString());
    }
    //public IActionResult Index()
    //{

    //    var products = _dataContext.Products.Include("Category").Include("Brand").ToList();
    //    var sliders = _dataContext.Sliders.Where(s => s.Status == 1).ToList();
    //    ViewBag.Sliders = sliders;
    //    return View(products);
    //}

    public IActionResult Index(int pg = 1, int? categoryId = null, int? brandId = null)
    {
        var productsQuery = _dataContext.Products
            .Include("Category")
            .Include("Brand");

        // Lọc theo category nếu có categoryId
        if (categoryId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Category.Id == categoryId.Value);
        }

        // Lọc theo brand nếu có brandId
        if (brandId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Brand.Id == brandId.Value);
        }

        var products = productsQuery.ToList();
        var sliders = _dataContext.Sliders
            .Where(s => s.Status == 1)
            .ToList();

        const int pageSize = 6;
        if (pg < 1)
        {
            pg = 1;
        }

        int recsCount = products.Count();
        var pager = new Paginate(recsCount, pg, pageSize);
        int recSkip = (pg - 1) * pageSize;

        var paginatedProducts = products.Skip(recSkip).Take(pager.PageSize).ToList();

        ViewBag.Pager = pager;
        ViewBag.Sliders = sliders;
        ViewBag.CategoryId = categoryId; // Lưu categoryId để sử dụng trong view
        ViewBag.BrandId = brandId;       // Lưu brandId để sử dụng trong view

        return View(paginatedProducts);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> Contact()
    {

        var contact = await _dataContext.Contacts.FirstAsync();
        return View(contact);
    }

    public async Task<IActionResult> Wishlist()
    {
        var wishlist_product = await (from w in _dataContext.Wishlists
                                      join p in _dataContext.Products on w.ProductId equals p.Id
                                      select new { Product = p, Wishlists = w })
                           .ToListAsync();

        return View(wishlist_product);
    }

    public async Task<IActionResult> AddWishlist(long Id, WishlistModel wishlistmodel)
    {
        var user = await _userManager.GetUserAsync(User);

        var wishlistProduct = new WishlistModel
        {
            ProductId = Id,
            UserId = user.Id
        };

        _dataContext.Wishlists.Add(wishlistProduct);
        try
        {
            await _dataContext.SaveChangesAsync();
            return Ok(new { success = true, message = "Add to wishlisht Successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while adding to wishlist table.");
        }

    }

    public async Task<IActionResult> DeleteWishlist(int Id)
    {
        WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);

        _dataContext.Wishlists.Remove(wishlist);

        await _dataContext.SaveChangesAsync();
        TempData["success"] = "Yêu thích đã được xóa thành công";
        return RedirectToAction("Wishlist", "Home");
    }

    public async Task<IActionResult> Compare()
    {
        var compare_product = await (from c in _dataContext.Compares
                                     join p in _dataContext.Products on c.ProductId equals p.Id
                                     join u in _dataContext.Users on c.UserId equals u.Id
                                     select new { User = u, Product = p, Compares = c })
                           .ToListAsync();

        return View(compare_product);
    }
    public async Task<IActionResult> AddCompare(long Id)
    {
        var user = await _userManager.GetUserAsync(User);

        var compareProduct = new CompareModel
        {
            ProductId = Id,
            UserId = user.Id
        };

        _dataContext.Compares.Add(compareProduct);
        try
        {
            await _dataContext.SaveChangesAsync();
            return Ok(new { success = true, message = "Add to compare Successfully" });
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while adding to compare table.");
        }

    }

    public async Task<IActionResult> DeleteCompare(int Id)
    {
        CompareModel compare = await _dataContext.Compares.FindAsync(Id);

        _dataContext.Compares.Remove(compare);

        await _dataContext.SaveChangesAsync();
        TempData["success"] = "So sánh đã được xóa thành công";
        return RedirectToAction("Compare", "Home");
    }

    public IActionResult About()
    {
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(int statuscode)
    {
        if (statuscode == 404)
        {
            return View("NotFound");
        }
        else{
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
     
    }
}
