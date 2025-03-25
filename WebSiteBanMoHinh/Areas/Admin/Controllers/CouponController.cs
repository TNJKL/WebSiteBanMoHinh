﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN ,AUTHOR")]
    public class CouponController : Controller
    {
        private readonly DataContext _dataContext;
        public CouponController(DataContext context)
        {
            _dataContext = context;

        }
        public async Task<IActionResult> Index()
        {
            var coupon_list = await _dataContext.Coupons.ToListAsync();
            ViewBag.Coupons = coupon_list;
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(CouponModel coupon)
        {


            if (ModelState.IsValid)
            {

                _dataContext.Add(coupon);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm coupon thành công";
                return RedirectToAction("Index");

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
                return BadRequest(errorMessage);
            }
            return View();
        }
    }
}
