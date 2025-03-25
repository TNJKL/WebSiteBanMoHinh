using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Models.ViewModels;
using WebSiteBanMoHinh.Repository;

namespace WebSiteBanMoHinh.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
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
        public async Task<IActionResult> ViewOrder(string orderCode)
        {
            //var DetailsOrder = await _dataContext.OrderDetails.Include(o => o.Product).Where(o => o.OrderCode == orderCode).ToListAsync();
            //return View(DetailsOrder);
            var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
               .Where(od => od.OrderCode == orderCode).ToListAsync();
            var ShippingCost = _dataContext.Orders.Where(o => o.OrderCode == orderCode).First();
            ViewBag.ShippingCost = ShippingCost.ShippingCost;

            var Order = _dataContext.Orders.Where(o => o.OrderCode == orderCode).First();

            // ViewBag.ShippingCost = Order.ShippingCost;
            ViewBag.Status = Order.Status;
            return View(DetailsOrder);
        }



        //[HttpPost]
        //public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        //{
        //    var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

        //    if (order == null)
        //    {
        //        return NotFound();
        //    }

        //    order.Status = status;

        //    try
        //    {
        //        await _dataContext.SaveChangesAsync();

        //        return Ok(new { success = true, message = "Order status updated successfully" });
        //    }
        //    catch (Exception)
        //    {


        //        return StatusCode(500, "An error occurred while updating the order status.");
        //    }
        //}


        //    [HttpPost]
        //    public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        //    {
        //        var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

        //        if (order == null)
        //        {
        //            return NotFound();
        //        }

        //        order.Status = status;
        //        _dataContext.Update(order);

        //        if (status == 0)
        //        {
        //            // Lấy dữ liệu order detail dựa vào order.OrderCode
        //            var DetailsOrder = await _dataContext.OrderDetails
        //                .Include(od => od.Product)
        //                .Where(od => od.OrderCode == order.OrderCode)
        //                .Select(od => new
        //                {
        //                    od.Quantity,
        //                    od.Product.Price,
        //                    od.Product.CapitalPrice
        //                }).ToListAsync();

        //            var statisticalModel = await _dataContext.Statisticals
        //.FirstOrDefaultAsync(s => s.CreatedDate.Date == order.CreatedDate.Date);

        //            if (statisticalModel != null)
        //            {
        //                foreach (var orderDetail in DetailsOrder)
        //                {
        //                    // Tồn tại ngày thì cộng dồn
        //                    statisticalModel.Quantity += 1;
        //                    statisticalModel.Sold += orderDetail.Quantity;
        //                    statisticalModel.Revenue += orderDetail.Quantity * orderDetail.Price;

        //                    statisticalModel.Profit += orderDetail.Price - orderDetail.CapitalPrice;
        //                }
        //                _dataContext.Update(statisticalModel);
        //            }
        //            else
        //            {
        //                int new_quantity = 0;
        //                int new_sold = 0;
        //                decimal new_profit = 0;

        //                foreach (var orderDetail in DetailsOrder)
        //                {
        //                    new_quantity += 1;
        //                    new_sold += orderDetail.Quantity;
        //                    new_profit += orderDetail.Price - orderDetail.CapitalPrice;

        //                    statisticalModel = new StatisticalModel
        //                    {
        //                        CreatedDate = order.CreatedDate,
        //                        Quantity = new_quantity,
        //                        Sold = new_sold,
        //                        Revenue = orderDetail.Quantity * orderDetail.Price,
        //                        Profit = new_profit
        //                    };
        //                }

        //                _dataContext.Add(statisticalModel);
        //            }
        //        }
        //        try
        //        {
        //            await _dataContext.SaveChangesAsync();
        //            return Ok(new { success = true, message = "Order status updated successfully" });
        //        }
        //        catch (Exception)
        //        {
        //            return StatusCode(500, "An error occured while updating the order status");
        //        }

        //    }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder(string ordercode, int status)
        {
            var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);

            if (order == null)
            {
                return NotFound();
            }

            order.Status = status;
            _dataContext.Update(order);

            if (status == 0)
            {
                // Lấy dữ liệu order detail dựa vào order.OrderCode
                var DetailsOrder = await _dataContext.OrderDetails
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
                    // Tồn tại ngày thì cộng dồn
                    //statisticalModel.Quantity = DetailsOrder.Sum(od => od.Quantity);
                    statisticalModel.Quantity += 1;

                    statisticalModel.Sold += DetailsOrder.Sum(od => od.Quantity);
                    statisticalModel.Revenue += DetailsOrder.Sum(od => od.Quantity * od.Price);
                    statisticalModel.Profit += DetailsOrder.Sum(od => od.Quantity * (od.Price - od.CapitalPrice));

                    _dataContext.Update(statisticalModel);
                }
                else
                {
                    // Tạo bản ghi mới nếu chưa có dữ liệu cho ngày này
                    statisticalModel = new StatisticalModel
                    {
                        CreatedDate = order.CreatedDate,
                        //Quantity = DetailsOrder.Sum(od => od.Quantity),
                        Quantity = 1,
                        Sold = DetailsOrder.Sum(od => od.Quantity),
                        Revenue = DetailsOrder.Sum(od => od.Quantity * od.Price),
                        Profit = DetailsOrder.Sum(od => od.Quantity * (od.Price - od.CapitalPrice))
                    };

                    _dataContext.Add(statisticalModel);
                }
            }

            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status updated successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the order status");
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

