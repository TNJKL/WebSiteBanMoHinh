using Microsoft.AspNetCore.Mvc;
using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Models.Vnpay;
using WebSiteBanMoHinh.Services.Momo;
using WebSiteBanMoHinh.Services.Vnpay;

namespace WebSiteBanMoHinh.Controllers
{
    public class PaymentController : Controller
    {
        
        private IMomoService _momoService;
        private readonly IVnPayService _vnPayService;

        public PaymentController(IMomoService momoService , IVnPayService vnPayService)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;


        }
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMomo(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentMomo(model);
            return Redirect(response.PayUrl);
        }


        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        }

        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
          

            return Redirect(url);
        }
       


    }
}
