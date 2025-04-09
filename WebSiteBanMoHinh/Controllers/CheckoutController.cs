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

        public CheckoutController(IEmailSender emailSender, DataContext context, IMomoService momoService, IVnPayService vnPayService)
        {
            _dataContext = context;
            _emailSender = emailSender;
            _momoService = momoService;
            _vnPayService = vnPayService;
        }

        public async Task<IActionResult> Checkout(string PaymentMethod, string PaymentId)
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if (userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orderCode = Guid.NewGuid().ToString();
            var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            double shippingPrice = shippingPriceCookie != null ? JsonConvert.DeserializeObject<double>(shippingPriceCookie) : 0;
            var coupon_code = Request.Cookies["CouponTitle"];

            // Lấy thông tin địa chỉ từ Session
            var address = HttpContext.Session.GetJSon<AddressModel>("ShippingAddress");
            if (address == null)
            {
                TempData["error"] = "Vui lòng nhập thông tin địa chỉ giao hàng.";
                return RedirectToAction("Index", "Cart");
            }

            var orderItem = new OrderModel
            {
                OrderCode = orderCode,
                ShippingCost = shippingPrice,
                CouponCode = coupon_code,
                UserName = userEmail,
                Status = 1,
                CreatedDate = DateTime.Now,
                PaymentMethod = PaymentMethod + " " + PaymentId,
                City = address.City,
                District = address.District,
                Ward = address.Ward,
                HouseNumber = address.HouseNumber
            };

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
                var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                product.Quantity -= cart.Quantity;
                product.Sold += cart.Quantity;
                _dataContext.Update(product);

                orderDetailsList.Add(orderDetails);
                _dataContext.Add(orderDetails);
                _dataContext.SaveChanges();

                total += cart.Price * cart.Quantity;
            }

            HttpContext.Session.Remove("Cart");
            Response.Cookies.Delete("ShippingPrice");
            HttpContext.Session.Remove("ShippingAddress");

            var emailBody = $@"
            <!DOCTYPE html>
            <html>
            <body style='font-family:Arial, sans-serif;'>
                <div style='max-width:600px; margin:0 auto;'>
                    <h2 style='color:#2d89ef;'>Đặt hàng thành công!</h2>
                    <p>Mã đơn hàng: <span style='color:#d9534f;'>{orderCode}</span></p>
                    <p>Địa chỉ giao hàng: {address.HouseNumber}, {address.Ward}, {address.District}, {address.City}</p>
                    <table style='width:100%;'>
                        <tr style='background-color:#2d89ef; color:#ffffff;'>
                            <th>Sản phẩm</th><th>Giá</th><th>Số lượng</th><th>Thành tiền</th>
                        </tr>";

            foreach (var item in orderDetailsList)
            {
                var product = _dataContext.Products.FirstOrDefault(p => p.Id == item.ProductId);
                if (product != null)
                {
                    emailBody += $@"
                    <tr>
                        <td>{product.Name}</td>
                        <td>{item.Price:C}</td>
                        <td>{item.Quantity}</td>
                        <td>{(item.Price * item.Quantity):C}</td>
                    </tr>";
                }
            }

            emailBody += $@"
                    </table>
                    <h3>Tổng tiền: {total + shippingPrice:C}</h3>
                </div>
            </body>
            </html>";

            await _emailSender.SendEmailAsync(userEmail, "Xác nhận đơn hàng thành công", emailBody, true);
            TempData["success"] = "Checkout thành công, vui lòng chờ duyệt đơn hàng!";
            return RedirectToAction("History", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallBack(MomoInfoModel model)
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
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
                await Checkout(PaymentMethod1, requestQuery["orderId"]);
            }
            else
            {
                TempData["error"] = "Đã hủy giao dịch Momo.";
                return RedirectToAction("Index", "Cart");
            }
            return View(response);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response.VnPayResponseCode == "00")
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
                await Checkout(PaymentMethod, PaymentId);
            }
            else
            {
                TempData["error"] = "Đã hủy giao dịch Vnpay.";
                return RedirectToAction("Index", "Cart");
            }
            return View(response);
        }
    }
}