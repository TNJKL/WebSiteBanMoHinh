namespace WebSiteBanMoHinh.Models.ViewModels
{
    public class PaymentVnpayViewModel
    { 
        //NHỚ PHẢI XÓA
        //ko đc thì xóa cho đỡ thừa !!!!
        public VnpayModel Vnpay { get; set; }
        public OrderModel Order { get; set; }
        public double TotalPrice { get; set; }
    }
}
