using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN ,EMPLOYEE")]
    public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        }
        //[HttpGet]
        //public IActionResult Add()
        //{
        //    ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "Name");
        //    ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "Name");
        //    return View();
        //}
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories.Where(c => c.Status == 1), "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands.Where(b => b.Status == 1), "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ProductModel product)
        {
            ViewBag.Categories = new SelectList(
                _dataContext.Categories.Where(c => c.Status == 1),
                "Id",
                "Name",
                product.CategoryId
            );
            ViewBag.Brands = new SelectList(
                _dataContext.Brands.Where(b => b.Status == 1),
                "Id",
                "Name",
                product.BrandId
            );

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");
                var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
                if (slug != null)
                {
                    ModelState.AddModelError("", "Sản phẩm đã tồn tại");
                    return View(product);
                }

                // Xử lý ảnh đại diện
                if (product.MainImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.MainImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.MainImageUpload.CopyToAsync(fs);
                    }
                    product.Image = imageName;
                }

                // Xử lý các ảnh bổ sung
                if (product.AdditionalImageUploads != null && product.AdditionalImageUploads.Length > 0)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    foreach (var imageUpload in product.AdditionalImageUploads)
                    {
                        if (imageUpload != null && imageUpload.Length > 0)
                        {
                            string imageName = Guid.NewGuid().ToString() + "_" + imageUpload.FileName;
                            string filePath = Path.Combine(uploadsDir, imageName);

                            using (var fs = new FileStream(filePath, FileMode.Create))
                            {
                                await imageUpload.CopyToAsync(fs);
                            }
                            product.Images.Add(new ProductImage { Url = imageName });
                        }
                    }
                }

                _dataContext.Add(product);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm sản phẩm thành công";
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

            return View(product);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(long Id)
        {
            ProductModel product = await _dataContext.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long Id, ProductModel product, int[] deleteImageIds)
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);
            var existingProduct = await _dataContext.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == Id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                product.Slug = product.Name.Replace(" ", "-");

                // Cập nhật thông tin sản phẩm
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.CapitalPrice = product.CapitalPrice;
                existingProduct.Price = product.Price;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;

                // Xử lý ảnh đại diện mới (nếu có)
                if (product.MainImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    string imageName = Guid.NewGuid().ToString() + "_" + product.MainImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    // Xóa ảnh đại diện cũ
                    if (!string.IsNullOrEmpty(existingProduct.Image))
                    {
                        string oldFilePath = Path.Combine(uploadsDir, existingProduct.Image);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    using (var fs = new FileStream(filePath, FileMode.Create))
                    {
                        await product.MainImageUpload.CopyToAsync(fs);
                    }
                    existingProduct.Image = imageName;
                }

                // Xử lý xóa ảnh bổ sung
                if (deleteImageIds != null && deleteImageIds.Any())
                {
                    var imagesToDelete = existingProduct.Images.Where(img => deleteImageIds.Contains((int)img.Id)).ToList();
                    foreach (var image in imagesToDelete)
                    {
                        string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                        string oldFilePath = Path.Combine(uploadsDir, image.Url);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                        existingProduct.Images.Remove(image);
                        _dataContext.ProductImages.Remove(image);
                    }
                }

                // Xử lý thêm ảnh bổ sung mới
                if (product.AdditionalImageUploads != null && product.AdditionalImageUploads.Length > 0)
                {
                    string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                    foreach (var imageUpload in product.AdditionalImageUploads)
                    {
                        if (imageUpload != null && imageUpload.Length > 0)
                        {
                            string imageName = Guid.NewGuid().ToString() + "_" + imageUpload.FileName;
                            string filePath = Path.Combine(uploadsDir, imageName);

                            using (var fs = new FileStream(filePath, FileMode.Create))
                            {
                                await imageUpload.CopyToAsync(fs);
                            }
                            existingProduct.Images.Add(new ProductImage { Url = imageName });
                        }
                    }
                }

                _dataContext.Update(existingProduct);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật sản phẩm thành công";
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

            return View(product);
        }

        public async Task<IActionResult> Delete(long Id)
        {
            ProductModel product = await _dataContext.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == Id);
            if (product == null)
            {
                TempData["error"] = "Sản phẩm không tồn tại";
                return RedirectToAction("Index");
            }

            // Xóa ảnh đại diện
            if (!string.IsNullOrEmpty(product.Image) && !string.Equals(product.Image, "noname.jpg"))
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                string oldFilePath = Path.Combine(uploadsDir, product.Image);
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }

            // Xóa các ảnh bổ sung
            if (product.Images != null && product.Images.Any())
            {
                string uploadsDir = Path.Combine(_webHostEnvironment.WebRootPath, "media/products");
                foreach (var image in product.Images)
                {
                    string oldFilePath = Path.Combine(uploadsDir, image.Url);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
            }

            _dataContext.Products.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Đã xóa sản phẩm thành công";
            return RedirectToAction("Index");
        }
        public async Task<IActionResult> AddQuantity(int Id)
        {
            var productByQuantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
            ViewBag.ProductByQuantity = productByQuantity;
            ViewBag.Id = Id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
        {
            // Get the product to update
            var product = _dataContext.Products.Find(productQuantityModel.ProductId);

            if (product == null)
            {
                return NotFound(); // Handle product not found scenario
            }
            product.Quantity += productQuantityModel.Quantity;

            productQuantityModel.Quantity = productQuantityModel.Quantity;
            productQuantityModel.ProductId = productQuantityModel.ProductId;
            productQuantityModel.DateCreate = DateTime.Now;


            _dataContext.Add(productQuantityModel);
            _dataContext.SaveChangesAsync();
            TempData["success"] = "Thêm số lượng sản phẩm thành công";
            return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId });
        }
    }
}
