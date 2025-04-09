namespace WebSiteBanMoHinh.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public double ShippingCost { get; set; }

        public string CouponCode { get; set; }
        public string? PaymentMethod { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        // Thêm các trường địa chỉ
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
        public string HouseNumber { get; set; }
    }
}
