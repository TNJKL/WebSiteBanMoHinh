using WebSiteBanMoHinh.Models;
using WebSiteBanMoHinh.Models.Momo;

namespace WebSiteBanMoHinh.Services.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);
        MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}
