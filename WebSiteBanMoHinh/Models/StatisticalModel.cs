﻿namespace WebSiteBanMoHinh.Models
{
    public class StatisticalModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public double Revenue { get; set; }
        public double Profit { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
