using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Models.ViewModels;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN ,EMPLOYEE")]
    public class OrderController : Controller
    {
        private readonly DataContext _dataContext;
        public OrderController(DataContext context)
        {
            _dataContext = context;

        }
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Orders.OrderByDescending(p => p.Id).ToListAsync());
        }
        //public async Task<IActionResult> ViewOrder(string orderCode)
        //{
        //    //var DetailsOrder = await _dataContext.OrderDetails.Include(o => o.Product).Where(o => o.OrderCode == orderCode).ToListAsync();
        //    //return View(DetailsOrder);
        //    var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
        //       .Where(od => od.OrderCode == orderCode).ToListAsync();
        //    var ShippingCost = _dataContext.Orders.Where(o => o.OrderCode == orderCode).First();
        //    ViewBag.ShippingCost = ShippingCost.ShippingCost;

        //    var Order = _dataContext.Orders.Where(o => o.OrderCode == orderCode).First();

        //    // ViewBag.ShippingCost = Order.ShippingCost;
        //    ViewBag.Status = Order.Status;
        //    return View(DetailsOrder);
        //}
        public async Task<IActionResult> ViewOrder(string orderCode)
        {
            var detailsOrder = await _dataContext.OrderDetails
                .Include(od => od.Product)
                .Where(od => od.OrderCode == orderCode)
                .ToListAsync();

            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

            if (order == null)
            {
                return NotFound();
            }

            // Gán giá trị cho ViewBag
            ViewBag.ShippingCost = order.ShippingCost;
            ViewBag.Status = order.Status; // Đảm bảo gán Status từ order

            return View(detailsOrder);
        }






        //[HttpPost]
        //public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        //{
        //    var order = await _dataContext.Orders
        //        .FirstOrDefaultAsync(o => o.OrderCode == ordercode);

        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    // Không cho phép thay đổi nếu đã hủy (3)
        //    if (order.Status == 3)
        //    {
        //        return BadRequest(new { success = false, message = "Không thể thay đổi trạng thái vì đơn hàng đã bị hủy." });
        //    }

        //    // Cập nhật trạng thái
        //    order.Status = status;
        //    _dataContext.Update(order);

        //    // Cập nhật dashboard khi chuyển sang "Đã xử lý" (0) và chưa xử lý trước đó
        //    if (status == 0 && order.Status != 0)
        //    {
        //        var detailsOrder = await _dataContext.OrderDetails
        //            .Include(od => od.Product)
        //            .Where(od => od.OrderCode == order.OrderCode)
        //            .Select(od => new
        //            {
        //                od.Quantity,
        //                od.Product.Price,
        //                od.Product.CapitalPrice
        //            }).ToListAsync();

        //        var statisticalModel = await _dataContext.Statisticals
        //            .FirstOrDefaultAsync(s => s.CreatedDate.Date == order.CreatedDate.Date);

        //        if (statisticalModel != null)
        //        {
        //            statisticalModel.Quantity += 1;
        //            statisticalModel.Sold += detailsOrder.Sum(od => od.Quantity);
        //            statisticalModel.Revenue += detailsOrder.Sum(od => od.Quantity * od.Price);
        //            statisticalModel.Profit += detailsOrder.Sum(od => od.Quantity * (od.Price - od.CapitalPrice));
        //            _dataContext.Update(statisticalModel);
        //        }
        //        else
        //        {
        //            statisticalModel = new StatisticalModel
        //            {
        //                CreatedDate = order.CreatedDate,
        //                Quantity = 1,
        //                Sold = detailsOrder.Sum(od => od.Quantity),
        //                Revenue = detailsOrder.Sum(od => od.Quantity * od.Price),
        //                Profit = detailsOrder.Sum(od => od.Quantity * (od.Price - od.CapitalPrice))
        //            };
        //            _dataContext.Add(statisticalModel);
        //        }
        //    }

        //    try
        //    {
        //        await _dataContext.SaveChangesAsync();
        //        return Ok(new { success = true, message = "Cập nhật trạng thái thành công." });
        //    }
        //    catch (Exception)
        //    {
        //        return StatusCode(500, "Lỗi khi cập nhật trạng thái.");
        //    }
        //}

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders
                .FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }

            // Không cho phép thay đổi nếu đã hủy (3)
            if (order.Status == 3)
            {
                return BadRequest(new { success = false, message = "Không thể thay đổi trạng thái vì đơn hàng đã bị hủy." });
            }

            // Lưu trạng thái cũ trước khi cập nhật
            var oldStatus = order.Status;

            // Cập nhật trạng thái mới
            order.Status = status;
            _dataContext.Update(order);

            // Cập nhật dashboard khi chuyển sang "Đã xử lý" (0) và trạng thái cũ không phải là "Đã xử lý"
            if (status == 0 && oldStatus != 0)
            {
                var detailsOrder = await _dataContext.OrderDetails
                    .Include(od => od.Product)
                    .Where(od => od.OrderCode == order.OrderCode)
                    .Select(od => new
                    {
                        od.Quantity,
                        od.Product.Price,
                        od.Product.CapitalPrice
                    }).ToListAsync();

                var statisticalModel = await _dataContext.Statisticals
                    .FirstOrDefaultAsync(s => s.CreatedDate.Date == order.CreatedDate.Date);

                if (statisticalModel != null)
                {
                    statisticalModel.Quantity += 1;
                    statisticalModel.Sold += detailsOrder.Sum(od => od.Quantity);
                    statisticalModel.Revenue += detailsOrder.Sum(od => od.Quantity * od.Price);
                    statisticalModel.Profit += detailsOrder.Sum(od => od.Quantity * (od.Price - od.CapitalPrice));
                    _dataContext.Update(statisticalModel);
                }
                else
                {
                    statisticalModel = new StatisticalModel
                    {
                        CreatedDate = order.CreatedDate,
                        Quantity = 1,
                        Sold = detailsOrder.Sum(od => od.Quantity),
                        Revenue = detailsOrder.Sum(od => od.Quantity * od.Price),
                        Profit = detailsOrder.Sum(od => od.Quantity * (od.Price - od.CapitalPrice))
                    };
                    _dataContext.Add(statisticalModel);
                }
            }

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Cập nhật trạng thái thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string ordercode)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }
            try
            {

                //delete order
                _dataContext.Orders.Remove(order);


                await _dataContext.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {

                return StatusCode(500, "An error occurred while deleting the order.");
            }
        }
        [HttpGet]
        public async Task<IActionResult> PaymentMomoInfo(string orderId)
        {
            var momoInfo = await _dataContext.MomoInfoModels.FirstOrDefaultAsync(m => m.OrderId == orderId);
            if (momoInfo == null)
            {
                return NotFound();
            }

            return View(momoInfo);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentVnpayInfo(string orderId)
        {
            var vnPayInfo = await _dataContext.VnpayInfos.FirstOrDefaultAsync(m => m.PaymentId == orderId);
            if (vnPayInfo == null)
            {
                return NotFound();
            }

            return View(vnPayInfo);
        }








    }
}

