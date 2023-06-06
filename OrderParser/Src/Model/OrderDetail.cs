using System;

namespace TextToOrder.Src.Model
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int LineNumber { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostEach { get; set; }
        public decimal CostTotal { get; set; }
        public string Description { get; set; }
    }
}

