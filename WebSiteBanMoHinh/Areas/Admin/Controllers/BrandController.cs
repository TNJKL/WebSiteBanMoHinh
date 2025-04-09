using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN ,EMPLOYEE")] 
    public class BrandController : Controller
    {
        private readonly DataContext _dataContext;
        public BrandController(DataContext context)
        {
            _dataContext = context;

        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Brands.OrderByDescending(p => p.Id).ToListAsync());
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(BrandModel brand)
        {
                
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Brand đã tồn tại");
                    return View(brand);
                }

                _dataContext.Add(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm brand thành công";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["error"] = "Model có một vài thứ đang bị lỗi";
                List<string> errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                string errorMessage = string.Join("\n", errors);
                return BadRequest(errorMessage);
            }

            return View(brand);
        }

        //public async Task<IActionResult> Delete(int Id)
        //{
        //    BrandModel brand = await _dataContext.Brands.FindAsync(Id);

        //    _dataContext.Brands.Remove(brand);
        //    await _dataContext.SaveChangesAsync();
        //    TempData["success"] = "Brand đã bị xóa";
        //    return RedirectToAction("Index");
        //}

        public async Task<IActionResult> Delete(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);

            if (brand == null)
            {
                return NotFound();
            }

            brand.Status = 0; // Giả sử bạn có thuộc tính Status trong BrandModel
            _dataContext.Brands.Update(brand); // Cập nhật bản ghi thay vì xóa
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Brand đã được ẩn";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Edit(int Id)
        {
            BrandModel brand = await _dataContext.Brands.FindAsync(Id);
            return View(brand);
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(BrandModel brand)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        brand.Slug = brand.Name.Replace(" ", "-");
        //        var slug = await _dataContext.Brands.FirstOrDefaultAsync(p => p.Slug == brand.Slug);
        //        if (slug != null)
        //        {
        //            ModelState.AddModelError("", "Brand đã tồn tại");
        //            return View(brand);
        //        }

        //        _dataContext.Update(brand);
        //        await _dataContext.SaveChangesAsync();
        //        TempData["success"] = "Cập nhật brand thành công";
        //        return RedirectToAction("Index");
        //    }
        //    else
        //    {
        //        TempData["error"] = "Model có một vài thứ đang bị lỗi";
        //        List<string> errors = new List<string>();
        //        foreach (var value in ModelState.Values)
        //        {
        //            foreach (var error in value.Errors)
        //            {
        //                errors.Add(error.ErrorMessage);
        //            }
        //        }
        //        string errorMessage = string.Join("\n", errors);
        //        return BadRequest(errorMessage);
        //    }

        //    return View(brand);
        //}


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BrandModel brand)
        {
            if (ModelState.IsValid)
            {
                brand.Slug = brand.Name.Replace(" ", "-");
                // Kiểm tra slug trùng lặp trong bảng Brands, loại trừ bản ghi hiện tại
                var existingBrand = await _dataContext.Brands
                    .FirstOrDefaultAsync(p => p.Slug == brand.Slug && p.Id != brand.Id);
                if (existingBrand != null)
                {
                    ModelState.AddModelError("", "Brand với slug này đã tồn tại");
                    return View(brand);
                }

                _dataContext.Update(brand);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật brand thành công";
                return RedirectToAction("Index");
            }
            // Xử lý lỗi ModelState
            TempData["error"] = "Model có một vài thứ đang bị lỗi";
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(string.Join("\n", errors));
        }
    }
}
