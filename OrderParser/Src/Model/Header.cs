using System;
using System.ComponentModel.DataAnnotations;

namespace TextToOrder.Src.Model
{
    public class Header
    {
        public int OrderNumber { get; set; }
        public int TotalItem { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } 
        public string CustomerPhone { get; set; } 
        public string CustomerEmail { get; set; } 
        public bool Paid { get; set; }
        public bool Shipped { get; set; }
        public bool Complete { get; set; }
    }
}

