﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebSiteBanMoHinh.Models
{
    public class VnpayModel
    {
        [Key]
        public int Id { get; set; }
        public string OrderDescription { get; set; }
        public string TransactionId { get; set; }
        public string OrderId { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentId { get; set; }
        public DateTime DateCreated{ get; set; }
        //lỗi thì xóa :P
        [NotMapped]
        public double TotalPrice { get; set; }
    }
}
