using System.ComponentModel.DataAnnotations.Schema;

namespace WebSiteBanMoHinh.Models.ViewModels
{
    public class CartItemViewModel
    {
        public  List<CartItemModel> CartItems { get; set; }

        public double GrandTotal { get; set; }
       
        public double ShippingCost { get; set; }
        public string CouponCode { get; set; }
    }
}
