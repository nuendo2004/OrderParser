using System;
using System.Collections.Generic;
using TextToOrder.Src.Model;

namespace OrderParser.Src
{
    public class Order
	{
		public int Id { get; set; }
		public Header Header { get; set; }
		public Address Address { get; set; }
		public List<OrderDetail> OrderDetails = new List<OrderDetail>();
		public string ErrorMessage { get; set; } = "";
		public bool IsValid { get; set; } = false;
			
	}
}
