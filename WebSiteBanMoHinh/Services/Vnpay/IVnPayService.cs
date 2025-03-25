using WebSiteBanMoHinh.Models.Vnpay;

namespace WebSiteBanMoHinh.Services.Vnpay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
