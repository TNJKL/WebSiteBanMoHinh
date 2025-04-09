using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN ,EMPLOYEE")]
    public class CategoryController : Controller
    {
        private readonly DataContext _dataContext;
        public CategoryController(DataContext context )
        {
            _dataContext = context;
            
        }
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> category = _dataContext.Categories.ToList();
            const int pageSize = 10;
            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = category.Count(); //33 items;

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
            //return View(await _dataContext.Categories.OrderByDescending(p => p.Id).ToListAsync());
        }
        public IActionResult Add()
        {
           
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CategoryModel category)
        {
           
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Danh mục đã tồn tại");
                    return View(category);
                }

                _dataContext.Add(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm danh mục thành công";
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

            return View(category);
        }

        public async Task<IActionResult> Edit(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);
            return View(category);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(CategoryModel category)
        //{

        //    if (ModelState.IsValid)
        //    {
        //        category.Slug = category.Name.Replace(" ", "-");
        //        var slug = await _dataContext.Categories.FirstOrDefaultAsync(p => p.Slug == category.Slug);
        //        if (slug != null)
        //        {
        //            ModelState.AddModelError("", "Danh mục đã tồn tại");
        //            return View(category);
        //        }

        //        _dataContext.Update(category);
        //        await _dataContext.SaveChangesAsync();
        //        TempData["success"] = "Cập nhật danh mục thành công";
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

        //    return View(category);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryModel category)
        {
            if (ModelState.IsValid)
            {
                category.Slug = category.Name.Replace(" ", "-");
                // Kiểm tra slug trùng lặp, nhưng loại trừ bản ghi hiện tại
                var existingCategory = await _dataContext.Categories
                    .FirstOrDefaultAsync(p => p.Slug == category.Slug && p.Id != category.Id);
                if (existingCategory != null)
                {
                    ModelState.AddModelError("", "Danh mục với slug này đã tồn tại");
                    return View(category);
                }

                _dataContext.Update(category);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật danh mục thành công";
                return RedirectToAction("Index");
            }
            // Xử lý lỗi ModelState
            TempData["error"] = "Model có một vài thứ đang bị lỗi";
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(string.Join("\n", errors));
        }
        //public async Task<IActionResult> Delete(int Id)
        //{
        //    CategoryModel category = await _dataContext.Categories.FindAsync(Id);

        //    _dataContext.Categories.Remove(category);
        //    await _dataContext.SaveChangesAsync();
        //    TempData["success"] = "Danh mục đã xóa";
        //    return RedirectToAction("Index");
        //}
        public async Task<IActionResult> Delete(int Id)
        {
            CategoryModel category = await _dataContext.Categories.FindAsync(Id);

            if (category == null)
            {
                return NotFound();
            }

            category.Status = 0; // Giả sử bạn có thuộc tính Status trong CategoryModel
            _dataContext.Categories.Update(category); // Cập nhật bản ghi thay vì xóa
            await _dataContext.SaveChangesAsync();

            TempData["success"] = "Danh mục đã được ẩn";
            return RedirectToAction("Index");
        }

    }
}
