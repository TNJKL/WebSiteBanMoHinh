using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Claims;
using WebSiteBanMoHinh.Areas.Admin.Repository;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Repository;
using WebSiteBanMoHinh.Services.Momo;
using WebSiteBanMoHinh.Services.Vnpay;

namespace WebSiteBanMoHinh.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IEmailSender _emailSender;
        private IMomoService _momoService;
        private readonly IVnPayService _vnPayService;


        public CheckoutController(IEmailSender emailSender, DataContext context , IMomoService momoService , IVnPayService vnPayService)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _momoService = momoService;
            _vnPayService = vnPayService;

        }
        //public async Task<IActionResult> Checkout()
        //{
        //    var userEmail = User.FindFirstValue(ClaimTypes.Email);
        //    if(userEmail == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }
        //    else
        //    {
        //        var orderCode = Guid.NewGuid().ToString();
        //        var orderItem = new OrderModel();
        //        orderItem.OrderCode = orderCode;
        //        orderItem.UserName = userEmail;
        //        orderItem.Status = 1;
        //        orderItem.CreatedDate = DateTime.Now;
        //        _dataContext.Add(orderItem);
        //        _dataContext.SaveChanges();
        //        List<CartItemModel> cartItems = HttpContext.Session.GetJSon<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
        //        foreach(var cart in cartItems)
        //        {
        //            var orderDetails = new OrderDetails();
        //            orderDetails.UserName = userEmail;
        //            orderDetails.OrderCode = orderCode;
        //            orderDetails.ProductId = cart.ProductId;
        //            orderDetails.Price = cart.Price;
        //            orderDetails.Quantity = cart.Quantity;
        //            _dataContext.Add(orderDetails);
        //            _dataContext.SaveChanges();
        //        }
        //        HttpContext.Session.Remove("Cart");
        //        var receiver = userEmail;
        //        var subject = "Đặt hàng thành công";
        //        var message = "Đặt hàng thành công, trải nghiệm dịch vụ nhé.";
        //        await _emailSender.SendEmailAsync(receiver, subject, message);
        //        TempData["success"] = "Checkout thành công , vui lòng chờ duyệt đơn hàng";
        //        return RedirectToAction("Index", "Cart");
        //    }
        //        return View();
        //}

        public async Task<IActionResult> Checkout(string PaymentMethod , string PaymentId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var orderCode = Guid.NewGuid().ToString();
                var shippingPriceCookie = Request.Cookies["ShippingPrice"];
                decimal shippingPrice = 0;
                //nhan coupon code
                var coupon_code = Request.Cookies["CouponTitle"];

                if (shippingPriceCookie != null)
                {
                    var shippingPriceJson = shippingPriceCookie;
                    shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
                }

                //vừa thêm vô nếu lỗi thì xóa
                else
                {
                    shippingPrice = 0;
                }

                //vừa thêm vô nếu lỗi thì xóa
                var orderItem = new OrderModel
                    {
                        OrderCode = orderCode,
                        ShippingCost = shippingPrice,
                        CouponCode = coupon_code,
                        UserName = userEmail,
                        Status = 1,
                        CreatedDate = DateTime.Now,
                        PaymentMethod = PaymentMethod + " " + PaymentId
                };
                //vừa thêm vô nếu lỗi thì xóa
                //if(PaymentMethod != "MOMO" || PaymentMethod != "VnPay")
                //{
                //    orderItem.PaymentMethod = "COD";
                //}
                //else if(PaymentMethod == "VnPay")
                //{
                //    orderItem.PaymentMethod = "VnPay" + " " + PaymentId;
                //}
                //else
                //{
                //    orderItem.PaymentMethod = "Momo" + " " + PaymentMethod;
                //}
                    //vừa thêm vô nếu lỗi thì xóa
                    _dataContext.Add(orderItem);
                _dataContext.SaveChanges();

                List<CartItemModel> cartItems = HttpContext.Session.GetJSon<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                double total = 0;
                List<OrderDetails> orderDetailsList = new List<OrderDetails>();

                foreach (var cart in cartItems)
                {
                    var orderDetails = new OrderDetails
                    {
                        UserName = userEmail,
                        OrderCode = orderCode,
                        ProductId = cart.ProductId,
                        Price = cart.Price,
                        Quantity = cart.Quantity
                    };
                    //
                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;
                    _dataContext.Update(product);

                    //
                    orderDetailsList.Add(orderDetails);
                    _dataContext.Add(orderDetails);
                    _dataContext.SaveChanges();

                    total += cart.Price * cart.Quantity;
                }

                // Xóa giỏ hàng sau khi đặt hàng thành công
                HttpContext.Session.Remove("Cart");

                // Tạo nội dung email với danh sách sản phẩm
                //var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var emailBody = $@"
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Xác nhận đơn hàng</title>
    </head>
    <body style='font-family:Arial, sans-serif; background-color:#f4f4f4; padding:20px;'>
        <div style='max-width:600px; margin:0 auto; background:#ffffff; padding:20px; border-radius:8px; box-shadow:0px 0px 10px rgba(0,0,0,0.1);'>
            <h2 style='color:#2d89ef; text-align:center;'>Đặt hàng thành công!</h2>
            <p style='font-size:16px; text-align:center;'>Cảm ơn bạn đã mua hàng! Dưới đây là thông tin đơn hàng của bạn:</p>
            <p><strong>Mã đơn hàng:</strong> <span style='color:#d9534f; font-size:18px;'>{orderCode}</span></p>

            <table style='width:100%; border-collapse:collapse; margin-top:10px;'>
                <tr style='background-color:#2d89ef; color:#ffffff;'>
                    <th style='padding:10px; text-align:left;'>Sản phẩm</th>
                    <th style='padding:10px; text-align:right;'>Giá</th>
                    <th style='padding:10px; text-align:center;'>Số lượng</th>
                    <th style='padding:10px; text-align:right;'>Thành tiền</th>
                </tr>";

                foreach (var item in orderDetailsList)
                {
                    var product = _dataContext.Products.FirstOrDefault(p => p.Id == item.ProductId);
                    if (product != null)
                    {
                        emailBody += $@"
                <tr style='border-bottom:1px solid #ddd;'>
                    <td style='padding:10px;'>{product.Name}</td>

      
                    <td style='padding:10px; text-align:right;'>{item.Price:C}</td>
                    <td style='padding:10px; text-align:center;'>{item.Quantity}</td>
                    <td style='padding:10px; text-align:right;'>{(item.Price * item.Quantity):C}</td>
                </tr>";
                    }
                }

                emailBody += $@"
            </table>
            <h3 style='text-align:right; color:#d9534f;'>Tổng tiền: {(total):C}</h3>
            <p style='text-align:center; font-size:14px;'>Cảm ơn bạn đã mua hàng! Hãy kiểm tra lại đơn hàng của mình.</p>
        </div>
    </body>
    </html>";



                var receiver = userEmail;
                var subject = "Xác nhận đơn hàng thành công";

                await _emailSender.SendEmailAsync(receiver, subject, emailBody, true);

                TempData["success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng!";
                return RedirectToAction("History", "Account");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack(MomoInfoModel model)
        {
            var response =  _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            var requestQuery = HttpContext.Request.Query;
            if (requestQuery["errorCode"] == "0")
            {
                var newMomoInsert = new MomoInfoModel
                {
                    OrderId = requestQuery["orderId"],
                    FullName = User.FindFirstValue(ClaimTypes.Email),
                    Amount = decimal.Parse(requestQuery["Amount"]),
                    OrderInfo = requestQuery["orderInfo"],
                    DatePaid = DateTime.Now
                };

                _dataContext.Add(newMomoInsert);
                await _dataContext.SaveChangesAsync();
                var PaymentMethod1 = "MOMO";
                // tiến hành đặt hàng khi thanh toàn bằng mô mô thành công 
                await Checkout(PaymentMethod1 ,requestQuery["orderId"]);   
            }
            else
            {
                TempData["error"] = "Đã hủy giao dịch Momo.";
                return RedirectToAction("Index", "Cart");


            }

             //
            return View(response);
        }


        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.VnPayResponseCode == "00") // giao dịch thành công lưu db
            {
                var newVnpayInsert = new VnpayModel
                {
                    OrderId = response.OrderId,
                    PaymentMethod = response.PaymentMethod,
                    OrderDescription = response.OrderDescription,
                    TransactionId = response.TransactionId,
                    PaymentId = response.PaymentId,
                    DateCreated = DateTime.Now
                };
                _dataContext.Add(newVnpayInsert);
                await _dataContext.SaveChangesAsync();
                var PaymentMethod = response.PaymentMethod;
                var PaymentId = response.PaymentId;
                await Checkout(PaymentMethod , PaymentId);

            }
            else
            {
                TempData["error"] = "Đã hủy giao dịch Vnpay.";
                return RedirectToAction("Index", "Cart");


            }
            //return Json(response);
            return View(response);
        }



    }
}
